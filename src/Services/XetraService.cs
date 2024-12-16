using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.XPath;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Finance.Net.Exceptions;
using Finance.Net.Extensions;
using Finance.Net.Interfaces;
using Finance.Net.Mappings;
using Finance.Net.Models.Xetra;
using Finance.Net.Models.Xetra.Dto;
using Finance.Net.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;

namespace Finance.Net.Services;

public class XetraService : IXetraService
{
	private readonly ILogger<XetraService> _logger;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly FinanceNetConfiguration _options;
	private readonly IMapper _mapper;
	private static ServiceProvider? s_serviceProvider;
	private readonly AsyncPolicy _retryPolicy;

	public XetraService(ILogger<XetraService> logger,
											IHttpClientFactory httpClientFactory,
											IOptions<FinanceNetConfiguration> options,
											IReadOnlyPolicyRegistry<string> policyRegistry)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		_retryPolicy = policyRegistry?.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy) ?? throw new ArgumentNullException(nameof(policyRegistry));

		// do not use IoC, so users can use Automapper independently
		var config = new MapperConfiguration(cfg => cfg.AddProfile<XetraInstrumentAutomapperProfile>());
		_mapper = config.CreateMapper();
	}

	/// <summary>
	/// Creates a service for interacting with the Xetra API.
	/// Provides methods for retrieving tradable instruments, market data, and other relevant information from Xetra.
	/// </summary>
	public static IXetraService Create()
	{
		return Create(new FinanceNetConfiguration());
	}

	/// <summary>
	/// Creates a service for interacting with the Xetra API.
	/// Provides methods for retrieving tradable instruments, market data, and other relevant information from Xetra.
	/// </summary>
	/// <param name="cfg">Configure .Net Finance. <see cref="FinanceNetConfiguration"/> ></param>
	public static IXetraService Create(FinanceNetConfiguration cfg)
	{
		if (s_serviceProvider == null)
		{
			var services = new ServiceCollection();
			services.AddFinanceNet(cfg);
			s_serviceProvider = services.BuildServiceProvider();
		}
		return s_serviceProvider.GetRequiredService<IXetraService>();
	}

	public async Task<IEnumerable<Instrument>> GetInstruments(CancellationToken token = default)
	{
		var httpClient = _httpClientFactory.CreateClient(Constants.XetraHttpClientName);
		try
		{
			return await _retryPolicy.ExecuteAsync(async () =>
			{
				var url = await GetDownloadUrl(token).ConfigureAwait(false);
				var response = await httpClient.GetAsync(url, token).ConfigureAwait(false);
				response.EnsureSuccessStatusCode();
				var config = new CsvConfiguration(CultureInfo.InvariantCulture)
				{
					HasHeaderRecord = true,
					Delimiter = ";",
				};

				using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
				using var csv = new CsvReader(reader, config);
				await csv.ReadAsync();
				await csv.ReadAsync();
				csv.Context.RegisterClassMap<XetraInstrumentsMapping>();
				var records = csv.GetRecords<InstrumentItem>().ToList();

				var instruments = _mapper.Map<List<Instrument>>(records);
				return instruments.IsNullOrEmpty() ? throw new FinanceNetException("All fields empty") : instruments;
			});
		}
		catch (Exception ex)
		{
			throw new FinanceNetException("Cannot fetch from Xetra", ex);
		}
	}

	private async Task<Uri?> GetDownloadUrl(CancellationToken token = default)
	{
		var httpClient = _httpClientFactory.CreateClient(Constants.XetraHttpClientName);
		var url = Constants.XetraInstrumentsUrl.ToLowerInvariant();
		var baseUri = new Uri(url);

		try
		{
			return await _retryPolicy.ExecuteAsync(async () =>
			{
				var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
				response.EnsureSuccessStatusCode();

				var htmlContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				_logger.LogDebug("htmlContent={HtmlContent}", htmlContent.Minify());
				var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(htmlContent);

				var hrefAttributes = document.DocumentElement.SelectNodes("//a[contains(@class, 'download') and contains(., 'All tradable instruments')]/@href")?.Select(e => e.NodeValue);
				if (hrefAttributes.Count() != 1)
				{
					throw new FinanceNetException($"Failed finding download link, found {hrefAttributes.Count()} links");
				}
				var relativeDownloadUrl = hrefAttributes.FirstOrDefault();
				return new Uri(baseUri, relativeDownloadUrl);
			});
		}
		catch (Exception ex)
		{
			throw new FinanceNetException($"Cannot fetch from {_options.DatahubIoDownloadUrlNasdaqListedSymbols}", ex);
		}
	}
}

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

namespace Finance.Net.Services;

internal class XetraService : IXetraService
{
	private readonly ILogger<IXetraService> _logger;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly FinanceNetConfiguration _options;
	private readonly IMapper _mapper;
	private static ServiceProvider? _serviceProvider = null;

	public XetraService(ILogger<IXetraService> logger, IHttpClientFactory httpClientFactory, IOptions<FinanceNetConfiguration> options)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));

		// do not use IoC, so users can use Automapper independently
		var config = new MapperConfiguration(cfg => cfg.AddProfile<XetraInstrumentAutomapperProfile>());
		_mapper = config.CreateMapper();
	}

	/// <summary>
	/// Creates a service for interacting with the Xetra API.
	/// Provides methods for retrieving tradable instruments, market data, and other relevant information from Xetra.
	/// </summary>
	/// <param name="cfg">Optional: Default values to configure .Net Finance. <see cref="FinanceNetConfiguration"/> ></param>
	public static IXetraService Create(FinanceNetConfiguration? cfg = null)
	{
		if (_serviceProvider == null)
		{
			var services = new ServiceCollection();
			services.AddFinanceServices(cfg);
			_serviceProvider = services.BuildServiceProvider();
		}
		return _serviceProvider.GetRequiredService<IXetraService>();
	}

	public async Task<IEnumerable<Instrument>> GetInstruments(CancellationToken token = default)
	{
		var httpClient = _httpClientFactory.CreateClient(Constants.XetraHttpClientName);
		try
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
			csv.Read();
			csv.Read();
			csv.Context.RegisterClassMap<XetraInstrumentsMapping>();
			var records = csv.GetRecords<InstrumentItem>()?.ToList();

			return _mapper.Map<List<Instrument>>(records);
		}
		catch (Exception ex)
		{
			throw new FinanceNetException($"Unable to get assests from Xetra", ex);
		}
	}

	private async Task<Uri?> GetDownloadUrl(CancellationToken token = default)
	{
		var httpClient = _httpClientFactory.CreateClient(Constants.XetraHttpClientName);
		var url = $"{Constants.XetraInstrumentsUrl}".ToLower();
		var baseUri = new Uri(url);
		for (int attempt = 1; attempt <= _options.HttpRetries; attempt++)
		{
			try
			{
				var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
				var response = await httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
				response.EnsureSuccessStatusCode();

				var htmlContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				_logger.LogDebug(() => $"htmlContent={htmlContent.Minify()}");
				var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(htmlContent);

				var urlNodes = document
					.Body.SelectNodes("//a[contains(@class, 'download') and contains(., 'All tradable instruments')]")
					.ToList();
				var xpath = "//a[contains(@class, 'download') and contains(., 'All tradable instruments')]/@href";
				var hrefAttributes = document.DocumentElement.SelectNodes(xpath)?.Select(e => e.NodeValue);
				if (hrefAttributes.Count() != 1)
				{
					throw new FinanceNetException($"Failed finding download link, found {hrefAttributes.Count()} links");
				}
				var relativeDownloadUrl = hrefAttributes.FirstOrDefault();
				var uri = new Uri(url);
				string baseAddress = $"{uri.Scheme}://{uri.Host}";
				var fullUri = new Uri(baseUri, relativeDownloadUrl);

				return fullUri;
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"{attempt} retry to download instruments from Xetra");
				_logger.LogDebug(() => $"url={url}, ex={ex}");
				await Task.Delay(TimeSpan.FromSeconds(1 * attempt));
			}
		}
		throw new FinanceNetException("No instruments url found on Xetra.com");
	}
}


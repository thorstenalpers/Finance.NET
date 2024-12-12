using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using DotNetFinance.Exceptions;
using DotNetFinance.Extensions;
using DotNetFinance.Interfaces;
using DotNetFinance.Mappings;
using DotNetFinance.Models.DatahubIo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetFinance.Services;

internal class DatahubIoService : IDatahubIoService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly DotNetFinanceConfiguration _options;
	private static ServiceProvider? _serviceProvider = null;

	public DatahubIoService(IHttpClientFactory httpClientFactory, IOptions<DotNetFinanceConfiguration> options)
	{
		_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
	}

	/// <summary>
	/// Creates a service for interacting with the OpenData API.
	/// Provides methods for retrieving financial instruments, market data, and other relevant information from OpenData.
	/// </summary>
	/// <param name="cfg">Optional: Default values to configure .Net Finance. <see cref="DotNetFinanceConfiguration"/> ></param>
	public static IDatahubIoService Create(DotNetFinanceConfiguration? cfg = null)
	{
		if (_serviceProvider == null)
		{
			var services = new ServiceCollection();
			services.AddFinanceServices(cfg);
			_serviceProvider = services.BuildServiceProvider();
		}
		return _serviceProvider.GetRequiredService<IDatahubIoService>();
	}

	public async Task<IEnumerable<NasdaqInstrument>> GetNasdaqInstrumentsAsync(CancellationToken token = default)
	{
		var httpClient = _httpClientFactory.CreateClient(_options.DatahubIoHttpClientName);
		try
		{
			var response = await httpClient.GetAsync(_options.DatahubIoDownloadUrlNasdaqListedSymbols, token);
			response.EnsureSuccessStatusCode();
			var config = new CsvConfiguration(CultureInfo.InvariantCulture);

			using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
			using var csv = new CsvReader(reader, config);
			csv.Context.RegisterClassMap<NasdaqInstrumentMapping>();
			return csv.GetRecords<NasdaqInstrument>().ToList();
		}
		catch (Exception ex)
		{
			throw new DotNetFinanceException($"Unable to download from {_options.DatahubIoDownloadUrlNasdaqListedSymbols}", ex);
		}
	}

	public async Task<IEnumerable<SP500Instrument>> GetSP500InstrumentsAsync(CancellationToken token = default)
	{
		var httpClient = _httpClientFactory.CreateClient(_options.DatahubIoHttpClientName);
		try
		{
			var response = await httpClient.GetAsync(_options.DatahubIoDownloadUrlSP500Symbols, token);
			response.EnsureSuccessStatusCode();
			var config = new CsvConfiguration(CultureInfo.InvariantCulture);

			using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
			using var csv = new CsvReader(reader, config);
			csv.Context.RegisterClassMap<SP500InstrumentMapping>();

			return csv.GetRecords<SP500Instrument>().ToList();
		}
		catch (Exception ex)
		{
			throw new DotNetFinanceException($"Unable to download from {_options.DatahubIoDownloadUrlSP500Symbols}", ex);
		}
	}
}
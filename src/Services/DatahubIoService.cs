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
using Finance.Net.Exceptions;
using Finance.Net.Extensions;
using Finance.Net.Interfaces;
using Finance.Net.Mappings;
using Finance.Net.Models.DatahubIo;
using Finance.Net.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;

namespace Finance.Net.Services;

internal class DatahubIoService(IHttpClientFactory httpClientFactory,
                                IOptions<FinanceNetConfiguration> options,
                                IReadOnlyPolicyRegistry<string> policyRegistry) : IDatahubIoService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly FinanceNetConfiguration _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private static ServiceProvider? s_serviceProvider;
    private readonly AsyncPolicy _retryPolicy = policyRegistry?.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy) ?? throw new ArgumentNullException(nameof(policyRegistry));

    /// <summary>
    /// Creates a service for interacting with the OpenData API.
    /// Provides methods for retrieving financial instruments, market data, and other relevant information from OpenData.
    /// </summary>
    public static IDatahubIoService Create()
    {
        return Create(new FinanceNetConfiguration());
    }

    /// <summary>
    /// Creates a service for interacting with the OpenData API.
    /// Provides methods for retrieving financial instruments, market data, and other relevant information from OpenData.
    /// </summary>
    /// <param name="cfg">Configure .Net Finance. <see cref="FinanceNetConfiguration"/> ></param>
    public static IDatahubIoService Create(FinanceNetConfiguration cfg)
    {
        if (s_serviceProvider == null)
        {
            var services = new ServiceCollection();
            services.AddFinanceNet(cfg);
            s_serviceProvider = services.BuildServiceProvider();
        }
        return s_serviceProvider.GetRequiredService<IDatahubIoService>();
    }

    public async Task<IEnumerable<NasdaqInstrument>> GetNasdaqInstrumentsAsync(CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.DatahubIoHttpClientName);
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await httpClient.GetAsync(_options.DatahubIoDownloadUrlNasdaqListedSymbols, token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var config = new CsvConfiguration(CultureInfo.InvariantCulture);

                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<NasdaqInstrumentMapping>();
                var instruments = csv.GetRecords<NasdaqInstrument>().ToList();
                return instruments.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : instruments;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"Unable to download from {_options.DatahubIoDownloadUrlNasdaqListedSymbols}", ex);
        }
    }

    public async Task<IEnumerable<SP500Instrument>> GetSP500InstrumentsAsync(CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.DatahubIoHttpClientName);
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await httpClient.GetAsync(_options.DatahubIoDownloadUrlSP500Symbols, token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var config = new CsvConfiguration(CultureInfo.InvariantCulture);

                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<SP500InstrumentMapping>();

                var instruments = csv.GetRecords<SP500Instrument>().ToList();
                return instruments.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : instruments;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"Unable to download from {_options.DatahubIoDownloadUrlSP500Symbols}", ex);
        }
    }
}
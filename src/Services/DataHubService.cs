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
using Finance.Net.Interfaces;
using Finance.Net.Mappings;
using Finance.Net.Models.DataHub;
using Finance.Net.Utilities;
using Polly;
using Polly.Registry;

namespace Finance.Net.Services;

/// <inheritdoc />
public class DataHubService(IHttpClientFactory httpClientFactory,
                                IReadOnlyPolicyRegistry<string> policyRegistry) : IDataHubService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly AsyncPolicy _retryPolicy = policyRegistry?.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy) ?? throw new ArgumentNullException(nameof(policyRegistry));

    /// <inheritdoc />
    public async Task<IEnumerable<NasdaqInstrument>> GetNasdaqInstrumentsAsync(CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.DatahubIoHttpClientName);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture);
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await httpClient.GetAsync(Constants.DatahubNasdaqSymbolsUrl, token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var reader = new StringReader(content);
                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<NasdaqInstrumentMapping>();
                var instruments = csv.GetRecords<NasdaqInstrument>().ToList();
                return instruments.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMessageAllFieldsEmpty) : instruments;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No instruments found", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Sp500Instrument>> GetSp500InstrumentsAsync(CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.DatahubIoHttpClientName);
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await httpClient.GetAsync(Constants.DatahubSp500SymbolsUrl, token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var config = new CsvConfiguration(CultureInfo.InvariantCulture);

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var reader = new StringReader(content);
                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<SP500InstrumentMapping>();

                var instruments = csv.GetRecords<Sp500Instrument>().ToList();
                return instruments.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMessageAllFieldsEmpty) : instruments;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No instruments found", ex);
        }
    }
}
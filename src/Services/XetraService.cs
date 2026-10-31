using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.XPath;
using CsvHelper;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Finance.Net.Mappings;
using Finance.Net.Models.Xetra;
using Finance.Net.Models.Xetra.Dto;
using Finance.Net.Utilities;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace Finance.Net.Services;

/// <inheritdoc />
public class XetraService : IXetraService
{
    private readonly ILogger<XetraService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AsyncPolicy _retryPolicy;

    /// <inheritdoc />
    public XetraService(ILogger<XetraService> logger,
                        IHttpClientFactory httpClientFactory,
                        IReadOnlyPolicyRegistry<string> policyRegistry)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _retryPolicy = policyRegistry?.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy) ?? throw new ArgumentNullException(nameof(policyRegistry));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Instrument>> GetInstrumentsAsync(CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.XetraHttpClientName);
        try
        {
            return await _retryPolicy.ExecuteAsync(async ct =>
            {
                var url = await GetDownloadUrl(ct).ConfigureAwait(false);
                var response = await httpClient.GetAsync(url, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                // CsvReaderOptions has init-only setters whose netstandard2.1 signature is missing from CsvHelper's net8.0+ build, so the defaults (separator detection) are used.
                using var csv = CsvReader.Create(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), null, leaveOpen: false);
                // The column header is the third line; the first two hold the market and the last update date.
                await csv.ReadAsync(ct).ConfigureAwait(false);
                if (!await csv.ReadAsync(ct).ConfigureAwait(false))
                {
                    throw new FinanceNetNoDataException("Xetra CSV contains no instrument rows");
                }
                await csv.ReadHeaderAsync(ct).ConfigureAwait(false);

                var records = new List<InstrumentItem>();
                await foreach (var record in csv.GetRecordsAsync<InstrumentItem>(ct))
                {
                    records.Add(record);
                }
                if (records.Count == 0)
                {
                    throw new FinanceNetNoDataException("Xetra CSV contains no instrument rows");
                }

                var result = records
                    .Select(record => record.ToInstrument())
                    .Where(instrument => !string.IsNullOrWhiteSpace(instrument.Mnemonic))
                    .ToList();
                return result.IsNullOrEmpty() ? throw new FinanceNetNoDataException("Xetra returned no instruments") : result;
            }, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is not FinanceNetNoDataException)
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
            return await _retryPolicy.ExecuteAsync(async ct =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, ct).ConfigureAwait(false);
                var hrefAttributes = document.DocumentElement.SelectNodes("//a[contains(@class, 'download') and contains(., 'All tradable instruments')]/@href")?.Select(e => e.NodeValue);
                var relativeDownloadUrl = hrefAttributes?.FirstOrDefault();
                return new Uri(baseUri, relativeDownloadUrl);
            }, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"Cannot fetch from {Constants.XetraInstrumentsUrl}", ex);
        }
    }
}

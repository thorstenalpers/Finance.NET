﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AutoMapper;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Finance.Net.Mappings;
using Finance.Net.Models.Yahoo;
using Finance.Net.Models.Yahoo.Dtos;
using Finance.Net.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;

namespace Finance.Net.Services;

/// <inheritdoc />
public class YahooFinanceService : IYahooFinanceService
{
    private readonly ILogger<YahooFinanceService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IYahooSessionManager _yahooSession;
    private readonly IMapper _mapper;
    private readonly AsyncPolicy _retryPolicy;

    /// <inheritdoc />
    public YahooFinanceService(
        ILogger<YahooFinanceService> logger,
        IHttpClientFactory httpClientFactory,
        IReadOnlyPolicyRegistry<string> policyRegistry,
        IYahooSessionManager yahooSession)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _yahooSession = yahooSession ?? throw new ArgumentNullException(nameof(yahooSession));
        _retryPolicy = policyRegistry?.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy) ?? throw new ArgumentNullException(nameof(policyRegistry));

        // do not use IoC, so users can use Automapper independently
        var config = new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapMethod = m => false;
            cfg.AddProfile<YahooQuoteAutomapperProfile>();
        });
        _mapper = config.CreateMapper();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Instrument>> GetInstrumentsAsync(EInstrumentType? filterByType = null, CancellationToken token = default)
    {
        var result = new List<Instrument>();
        var instrumentTypes = Enum.GetValues(typeof(EInstrumentType))
                                         .Cast<EInstrumentType>()
                                         .ToList();

        var typesToProcess = filterByType == null ? instrumentTypes : [filterByType.Value];

        foreach (var instrumentType in typesToProcess)
        {
            try
            {
                var instruments = await (instrumentType switch
                {
                    EInstrumentType.ETF => FetchSymbolsAsync($"{Constants.YahooBaseUrl}/markets/etfs/most-active/", EInstrumentType.ETF, token),
                    EInstrumentType.Stock => FetchSymbolsAsync($"{Constants.YahooBaseUrl}/markets/stocks/most-active/", EInstrumentType.Stock, token),
                    EInstrumentType.Forex => FetchSymbolsAsync($"{Constants.YahooBaseUrl}/markets/currencies/", EInstrumentType.Forex, token),
                    EInstrumentType.Crypto => FetchSymbolsAsync($"{Constants.YahooBaseUrl}/markets/crypto/all/", EInstrumentType.Crypto, token),
                    EInstrumentType.Index => FetchSymbolsAsync($"{Constants.YahooBaseUrl}/markets/world-indices/", EInstrumentType.Index, token),
                    _ => throw new NotSupportedException()
                }).ConfigureAwait(false);
                result.AddRange(instruments);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load {Type}", instrumentType);
            }
        }
        return result.IsNullOrEmpty() ? throw new FinanceNetException("No instruments found") : result;
    }

    /// <inheritdoc />
    public async Task<Quote> GetQuoteAsync(string symbol, CancellationToken token = default)
    {
        var quotes = await GetQuotesAsync([symbol], token).ConfigureAwait(false);
        return quotes.FirstOrDefault(e => e.Symbol == symbol);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Quote>> GetQuotesAsync(List<string> symbols, CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var crumb = _yahooSession.GetApiCrumb();
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var url = $"{Constants.YahooQuoteApiUrl}?" +
            $"&symbols={string.Join(",", symbols).ToLowerInvariant()}" +
            $"&crumb={crumb}";
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var quotes = new List<Quote>();

                var jsonContent = await Helper.FetchJsonDocumentAsync(httpClient, _logger, url, token).ConfigureAwait(false);
                var parsedData = JsonConvert.DeserializeObject<QuoteResponseRoot>(jsonContent) ?? throw new FinanceNetException("Invalid data returned by Yahoo");
                var responseObj = parsedData.QuoteResponse ?? throw new FinanceNetException("Invalid content from Yahoo");

                var error = responseObj.Error;
                if (error != null)
                {
                    throw new FinanceNetException($"Received an error response from Yahoo: {error}");
                }
                if (responseObj.Result == null || responseObj.Result.Length == 0)
                {
                    throw new FinanceNetException("No response from Yahoo");
                }

                foreach (var quoteResponse in responseObj.Result)
                {
                    if (quoteResponse.Symbol == null)
                    {
                        throw new FinanceNetException("Invalid quote field symbol");
                    }
                    var quote = _mapper.Map<Quote>(quoteResponse);
                    quotes.Add(quote);
                }
                return quotes;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No way to fetch quotes", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Record>> GetRecordsAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null, CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);

        startDate ??= DateTime.UtcNow.AddDays(-7).Date;

        endDate ??= DateTime.UtcNow.Date;
        endDate = endDate.Value.AddDays(1).Date;

        var period1 = Helper.ToUnixTime(startDate.Value.Date);
        var period2 = Helper.ToUnixTime(endDate.Value.Date);

        var url = $"{Constants.YahooQuoteHtmlUrl}/{symbol}/history/?period1={period1}&period2={period2}".ToLowerInvariant();
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token).ConfigureAwait(false);

                await CheckAndDeclineConsentAsync(document, token).ConfigureAwait(false);
                var records = YahooHtmlParser.ParseHistoryRecords(document, _logger);
                return records.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMessageAllFieldsEmpty) : records;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No records found", ex);
        }
    }

    private async Task CheckAndDeclineConsentAsync(IHtmlDocument document, CancellationToken token)
    {
        var title = document.QuerySelector("title")?.TextContent ?? "";
        if (title.Contains("Lookup"))
        {
            _yahooSession.InvalidateSession();
            await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);

            throw new FinanceNetException("Yahoo error: lookup received");
        }

        var consentDiv = document.QuerySelector("div#consent-page");
        if (consentDiv != null)
        {
            await _yahooSession.DeclineConsentAsync(document, token).ConfigureAwait(false);
            _logger.LogInformation("Consent declined");
        }
    }

    /// <inheritdoc />
    public async Task<Models.Yahoo.Profile> GetProfileAsync(string symbol, CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var url = $"{Constants.YahooQuoteHtmlUrl}/{symbol}/profile/".ToLowerInvariant();

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token).ConfigureAwait(false);
                await CheckAndDeclineConsentAsync(document, token).ConfigureAwait(false);
                var result = YahooHtmlParser.ParseProfile(document);
                return result;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No profile found", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, FinancialReport>> GetFinancialsAsync(string symbol, CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var url = $"{Constants.YahooQuoteHtmlUrl}/{symbol}/financials/".ToLowerInvariant();

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token).ConfigureAwait(false);
                await CheckAndDeclineConsentAsync(document, token).ConfigureAwait(false);
                var result = YahooHtmlParser.ParseFinancialReports(document, _logger);
                return result;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No financial reports found", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Summary> GetSummaryAsync(string symbol, CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var url = $"{Constants.YahooQuoteHtmlUrl}/{symbol}/?nojs=true".ToLowerInvariant();

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token).ConfigureAwait(false);
                await CheckAndDeclineConsentAsync(document, token).ConfigureAwait(false);
                var result = YahooHtmlParser.ParseSummary(document, _logger);
                return result;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No summary found", ex);
        }
    }

    private async Task<IEnumerable<Instrument>> FetchSymbolsAsync(string baseUrl, EInstrumentType instrumentType, CancellationToken token = default)
    {
        var result = new List<Instrument>();
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);

        try
        {
            for (var i = 0; i < 100; i++)
            {
                var url = $"{baseUrl}?start={i * 100}&count=100".ToLowerInvariant();
                var items = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token).ConfigureAwait(false);
                    await CheckAndDeclineConsentAsync(document, token).ConfigureAwait(false);
                    var parsed = YahooHtmlParser.ParseSymbols(document, instrumentType, _logger);
                    return Helper.AreAllPropertiesNull(parsed)
                        ? throw new FinanceNetException(Constants.ValidationMessageAllFieldsEmpty)
                        : parsed;
                }).ConfigureAwait(false);

                result.AddRange(items.Where(e => e.Symbol != null));
                result = result
                    .GroupBy(stock => stock.Symbol)
                    .Select(group => group.First())
                    .ToList();

                if (items.Count < 100)
                {
                    break;
                }
            }

            return result;
        }
        catch
        {
            if (result.IsNullOrEmpty())
            {
                throw;
            }
            else
            {
                return result;
            }
        }
    }

    /// <inheritdoc />
    public void InvalidateSession()
    {
        _yahooSession.InvalidateSession();
    }
}
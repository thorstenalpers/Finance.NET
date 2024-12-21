using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Extensions;
using Finance.Net.Interfaces;
using Finance.Net.Mappings;
using Finance.Net.Models.Yahoo;
using Finance.Net.Models.Yahoo.Dtos;
using Finance.Net.Utilities;
using Microsoft.Extensions.DependencyInjection;
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
    private static ServiceProvider? s_staticServiceProvider;
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
        var config = new MapperConfiguration(cfg => cfg.AddProfile<YahooQuoteAutomapperProfile>());
        _mapper = config.CreateMapper();
    }

    /// <summary>
    /// Creates a service for interacting with the Yahoo Finance API.
    /// Provides methods for retrieving historical data, company profiles, summaries, and financial reports from Yahoo Finance.
    /// </summary>
    public static IYahooFinanceService Create()
    {
        return Create(new FinanceNetConfiguration());
    }

    /// <summary>
    /// Creates a service for interacting with the Yahoo Finance API.
    /// Provides methods for retrieving historical data, company profiles, summaries, and financial reports from Yahoo Finance.
    /// </summary>
    /// <param name="cfg">Configure .Net Finance. <see cref="FinanceNetConfiguration"/> ></param>
    public static IYahooFinanceService Create(FinanceNetConfiguration cfg)
    {
        if (s_staticServiceProvider == null)
        {
            var services = new ServiceCollection();
            services.AddFinanceNet(cfg);
            s_staticServiceProvider = services.BuildServiceProvider();
        }
        return s_staticServiceProvider.GetRequiredService<IYahooFinanceService>();
    }

    /// <inheritdoc />
    public async Task<Quote> GetQuoteAsync(string symbol, CancellationToken token = default)
    {
        var symbols = await GetQuotesAsync([symbol], token);
        return symbols.FirstOrDefault(e => e.Symbol == symbol);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Quote>> GetQuotesAsync(List<string> symbols, CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var crumb = _yahooSession.GetApiCrumb();
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var url = $"{Constants.YahooBaseUrlQuoteApi}?" +
            $"&symbols={string.Join(",", symbols).ToLowerInvariant()}" +
            $"&crumb={crumb}";
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var quotes = new List<Quote>();

                var jsonContent = await Helper.FetchJsonDocumentAsync(httpClient, _logger, url, token);
                var parsedData = JsonConvert.DeserializeObject<QuoteResponseRoot>(jsonContent) ?? throw new FinanceNetException("Invalid data returned by Yahoo");
                var responseObj = parsedData.QuoteResponse ?? throw new FinanceNetException("Unexpected response from Yahoo");

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
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No way to fetch quotes", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Models.Yahoo.Profile> GetProfileAsync(string symbol, CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var url = $"{Constants.YahooBaseUrlQuoteHtml}/{symbol}/profile/".ToLowerInvariant();

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                var result = YahooHtmlParser.ParseProfile(document, _logger);
                return Helper.AreAllPropertiesNull(result) ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No profile found", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HistoryRecord>> GetRecordsAsync(string symbol, DateTime startDate, DateTime? endDate = null, CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);

        endDate ??= DateTime.UtcNow.Date;
        endDate = endDate.Value.AddDays(1).Date;

        var period1 = Helper.ToUnixTime(startDate.Date) ?? throw new FinanceNetException("Invalid startDate");
        var period2 = Helper.ToUnixTime(endDate.Value.Date) ?? throw new FinanceNetException("Invalid endDate");

        var url = $"{Constants.YahooBaseUrlQuoteHtml}/{symbol}/history/?period1={period1}&period2={period2}".ToLowerInvariant();

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                var records = YahooHtmlParser.ParseHistoryRecords(document, _logger);
                return records.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : records;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No records found", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, FinancialReport>> GetFinancialReportsAsync(string symbol, CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var url = $"{Constants.YahooBaseUrlQuoteHtml}/{symbol}/financials/".ToLowerInvariant();

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                var result = YahooHtmlParser.ParseFinancialReports(document, _logger);
                return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
            });
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
        var url = $"{Constants.YahooBaseUrlQuoteHtml}/{symbol}/".ToLowerInvariant();

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                var result = YahooHtmlParser.ParseSummary(document, _logger);
                return Helper.AreAllPropertiesNull(result) ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No summary found", ex);
        }
    }

    private async Task<IEnumerable<SymbolInfo>> GetCryptosAsync(CancellationToken token = default)
    {
        var result = new List<SymbolInfo>();
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var baseUrl = $"{Constants.YahooBaseUrlHtml}/markets/crypto/all/".ToLowerInvariant();
        try
        {
            for (var i = 0; i < 100; i++)
            {
                var url = $"{baseUrl}?start={i * 100}&count={100}".ToLowerInvariant();
                var items = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                    var result = YahooHtmlParser.ParseCryptos(document, _logger);
                    return Helper.AreAllPropertiesNull(result) ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
                });
                result.AddRange(items.Where(e => e.Symbol != null));
                result = result
                    .GroupBy(stock => stock.Symbol)
                    .Select(group => group.First())
                    .ToList();
                if (items.Count < 100)
                {
                    return result;
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            return !result.IsNullOrEmpty() ? (IEnumerable<SymbolInfo>)result : throw new FinanceNetException("No cryptos found", ex);
        }
    }

    private async Task<IEnumerable<SymbolInfo>> GetStocksAsync(CancellationToken token = default)
    {
        var result = new List<SymbolInfo>();
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var baseUrl = $"{Constants.YahooBaseUrlHtml}/markets/stocks/most-active/".ToLowerInvariant();
        try
        {
            for (var i = 0; i < 100; i++)
            {
                var url = $"{baseUrl}?start={i * 100}&count={100}".ToLowerInvariant();
                var items = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                    var result = YahooHtmlParser.ParseStocks(document, _logger);
                    return Helper.AreAllPropertiesNull(result) ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
                });
                result.AddRange(items.Where(e => e.Symbol != null));
                result = result
                    .GroupBy(stock => stock.Symbol)
                    .Select(group => group.First())
                    .ToList();
                if (items.Count < 100)
                {
                    return result;
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            return !result.IsNullOrEmpty() ? (IEnumerable<SymbolInfo>)result : throw new FinanceNetException("No stocks found", ex);
        }
    }

    private async Task<IEnumerable<SymbolInfo>> GetForexAsync(CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var url = $"{Constants.YahooBaseUrlHtml}/markets/currencies/".ToLowerInvariant();

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                var result = YahooHtmlParser.ParseForex(document, _logger);
                return Helper.AreAllPropertiesNull(result) ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No currencies found", ex);
        }
    }

    private async Task<IEnumerable<SymbolInfo>> GetIndicesAsync(CancellationToken token = default)
    {
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var url = $"{Constants.YahooBaseUrlHtml}/markets/world-indices/".ToLowerInvariant();

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                var result = YahooHtmlParser.ParseIndices(document, _logger);
                return Helper.AreAllPropertiesNull(result) ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException("No World Indices found", ex);
        }
    }

    private async Task<IEnumerable<SymbolInfo>> GetETFsAsync(CancellationToken token = default)
    {
        var result = new List<SymbolInfo>();
        await _yahooSession.RefreshSessionAsync(token).ConfigureAwait(false);
        var httpClient = _httpClientFactory.CreateClient(Constants.YahooHttpClientName);
        var baseUrl = $"{Constants.YahooBaseUrlHtml}/markets/etfs/most-active/".ToLowerInvariant();
        try
        {
            for (var i = 0; i < 100; i++)
            {
                var url = $"{baseUrl}?start={i * 100}&count={100}".ToLowerInvariant();
                var items = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var document = await Helper.FetchHtmlDocumentAsync(httpClient, _logger, url, token);
                    var result = YahooHtmlParser.ParseETFs(document, _logger);
                    return Helper.AreAllPropertiesNull(result) ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
                });
                result.AddRange(items.Where(e => e.Symbol != null));
                result = result
                    .GroupBy(stock => stock.Symbol)
                    .Select(group => group.First())
                    .ToList();
                if (items.Count < 100)
                {
                    return result;
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            return !result.IsNullOrEmpty() ? (IEnumerable<SymbolInfo>)result : throw new FinanceNetException("No ETFs found", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SymbolInfo>> GetSymbolsAsync(EInstrumentType? type = null, CancellationToken token = default)
    {
        var result = new List<SymbolInfo>();
        await Task.Delay(TimeSpan.FromSeconds(1), token);

        var instrumentMethods = new Dictionary<EInstrumentType, Func<CancellationToken, Task<IEnumerable<SymbolInfo>>>>
    {
        { EInstrumentType.Stock, GetStocksAsync },
        { EInstrumentType.ETF, GetETFsAsync },
        { EInstrumentType.Crypto, GetCryptosAsync },
        { EInstrumentType.Index, GetIndicesAsync },
        { EInstrumentType.Forex, GetForexAsync }
    };

        // If type is null, process all instrument types
        var typesToProcess = type.HasValue ? [type.Value] : instrumentMethods.Keys.ToList();

        foreach (var instrumentType in typesToProcess)
        {
            try
            {
                var symbols = await instrumentMethods[instrumentType](token);
                result.AddRange(symbols);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to load {Type}: {Ex}", instrumentType, ex?.Message);
            }
        }

        return result.IsNullOrEmpty() ? throw new FinanceNetException("No symbols found") : (IEnumerable<SymbolInfo>)result;
    }
}
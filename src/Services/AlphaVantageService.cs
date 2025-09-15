﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Finance.Net.Models.AlphaVantage;
using Finance.Net.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;

namespace Finance.Net.Services;
/// <inheritdoc />
public class AlphaVantageService : IAlphaVantageService
{
    private readonly ILogger<AlphaVantageService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FinanceNetConfiguration _options;
    private readonly IAsyncPolicy _retryPolicy;

    /// <inheritdoc />
    public AlphaVantageService(
        ILogger<AlphaVantageService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<FinanceNetConfiguration> options,
        IReadOnlyPolicyRegistry<string>? policyRegistry = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (policyRegistry != null &&
            policyRegistry.TryGet<IAsyncPolicy>(Constants.DefaultHttpRetryPolicy, out var found))
        {
            _retryPolicy = found;
        }
        else
        {
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
                    (ex, ts, retryCount, ctx) =>
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} wegen {Reason}, warte {Delay}s",
                            retryCount,
                            ex?.Message ?? "Unbekannt",
                            ts.TotalSeconds);
                    });

            _logger.LogWarning(
                "Retry-Policy '{PolicyKey}' nicht gefunden – verwende Default (3x Retry, Backoff).",
                Constants.DefaultHttpRetryPolicy);
        }
    }

    /// <inheritdoc />
    public async Task<InstrumentOverview?> GetOverviewAsync(string symbol, CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.AlphaVantageHttpClientName);
        var url = Constants.AlphaVantageApiBaseUrl + "/query?function=OVERVIEW" +
            $"&symbol={symbol}" +
            $"&apikey={_options.AlphaVantageApiKey}";

        try
        {
            var instrumentOverview = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var httpResponse = await httpClient.GetAsync(url, token).ConfigureAwait(false);
                    httpResponse.EnsureSuccessStatusCode();

                    var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (jsonResponse.Contains(Constants.ApiResponseLimitExceeded))
                    {
                        throw new FinanceNetException($"{Constants.ApiResponseLimitExceeded} for {symbol}");
                    }
                    else
                    {
                        var overview = JsonConvert.DeserializeObject<InstrumentOverview>(jsonResponse);
                        var isNullObj = Helper.AreAllPropertiesNull(overview);
                        return isNullObj ? throw new FinanceNetException(Constants.ValidationMessageAllFieldsEmpty) : overview;
                    }
                }).ConfigureAwait(false);
            return instrumentOverview;
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No overview found for {symbol}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Record>> GetRecordsAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null, CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.AlphaVantageHttpClientName);
        var url = Constants.AlphaVantageApiBaseUrl + "/query?function=TIME_SERIES_DAILY_ADJUSTED" +
            $"&symbol={symbol}&outputsize=full&apikey={_options.AlphaVantageApiKey}";
        Guard.Against.NullOrEmpty(symbol);

        startDate ??= DateTime.UtcNow.AddDays(-7).Date;
        endDate ??= DateTime.UtcNow;
        if (startDate > endDate)
        {
            throw new FinanceNetException("startDate earlier than endDate");
        }
        if (endDate.Value.Date >= DateTime.UtcNow.Date)
        {
            endDate = DateTime.UtcNow.Date;
        }
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var jsonResponse = await Helper.FetchJsonDocumentAsync(httpClient, _logger, url, token).ConfigureAwait(false);
                if (jsonResponse.Contains(Constants.ApiResponseLimitExceeded))
                {
                    throw new FinanceNetException($"{Constants.ApiResponseLimitExceeded} for {symbol}");
                }
                var result = AlphaVantageParser.ParseRecords(symbol, startDate, endDate, jsonResponse, _logger);
                return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMessageAllFieldsEmpty) : result;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No Record found for {symbol}", ex);
        }
    }



    /// <inheritdoc />
    public async Task<IEnumerable<IntradayRecord>> GetIntradayRecordsAsync(string symbol, DateTime startDate, DateTime? endDate = null, EInterval interval = EInterval.Interval_15Min, CancellationToken token = default)
    {
        Guard.Against.NullOrEmpty(symbol);
        var result = new List<IntradayRecord>();
        if (startDate > endDate)
        {
            throw new FinanceNetException("startDate earlier than endDate");
        }
        endDate ??= DateTime.UtcNow.Date;
        if (endDate.Value.Date >= DateTime.UtcNow.Date)
        {
            endDate = DateTime.UtcNow.Date;
        }

        for (var currentMonth = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc); currentMonth <= endDate; currentMonth = currentMonth.AddMonths(1))
        {
            if (currentMonth == endDate &&
                ((endDate.Value.Day == 1 && endDate.Value.DayOfWeek == DayOfWeek.Saturday) ||
                    (endDate.Value.Day == 1 && endDate.Value.DayOfWeek == DayOfWeek.Sunday) ||
                    (endDate.Value.Day == 2 && endDate.Value.DayOfWeek == DayOfWeek.Sunday)))
            {
                // dont query for data which not exists (api exception)
                break;
            }
            var currentCourses = await GetIntradayRecordsByMonthAsync(symbol, currentMonth, interval, token).ConfigureAwait(false);
            result.AddRange(currentCourses);
        }
        result = result.Where(e => e.DateTime >= startDate).ToList();
        return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMessageAllFieldsEmpty) : result;
    }

    private async Task<List<IntradayRecord>> GetIntradayRecordsByMonthAsync(string symbol, DateTime month, EInterval interval, CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.AlphaVantageHttpClientName);
        var url = Constants.AlphaVantageApiBaseUrl + "/query?function=TIME_SERIES_INTRADAY" +
            $"&symbol={symbol}" +
            $"&interval={interval.GetDescription()}" +
            $"&month={month:yyyy-MM}" +
            "&extended_hours=false" +      // no pre and post market trading
            "&outputsize=full" +
            $"&apikey={_options.AlphaVantageApiKey}";

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await httpClient.GetAsync(url, token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (jsonResponse.Contains(Constants.ApiResponseLimitExceeded))
                {
                    throw new FinanceNetException($"{Constants.ApiResponseLimitExceeded} for {symbol}");
                }
                var result = AlphaVantageParser.ParseIntradayRecords(symbol, interval, jsonResponse);
                return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMessageAllFieldsEmpty) : result;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No intraday record found for {symbol}", ex);
        }
    }


    /// <inheritdoc />
    public async Task<IEnumerable<ForexRecord>> GetForexRecordsAsync(string currency1, string currency2, DateTime startDate, DateTime? endDate = null, CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.AlphaVantageHttpClientName);
        Guard.Against.NullOrEmpty(currency1);
        Guard.Against.NullOrEmpty(currency2);
        if (startDate > endDate)
        {
            throw new FinanceNetException("startDate earlier than endDate");
        }

        endDate ??= DateTime.UtcNow.Date;
        if (endDate.Value.Date >= DateTime.UtcNow.Date)
        {
            endDate = DateTime.UtcNow.Date;
        }
        var url = Constants.AlphaVantageApiBaseUrl + "/query?function=FX_DAILY" +
            $"&from_symbol={currency1}&to_symbol={currency2}&outputsize=full&apikey={_options.AlphaVantageApiKey}";

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var jsonResponse = await Helper.FetchJsonDocumentAsync(httpClient, _logger, url, token).ConfigureAwait(false);
                if (jsonResponse.Contains(Constants.ApiResponseLimitExceeded))
                {
                    throw new FinanceNetException($"{Constants.ApiResponseLimitExceeded} for {currency1} /{currency2}");
                }
                var result = AlphaVantageParser.ParseForexRecords(currency1, currency2, startDate, endDate, jsonResponse, _logger);
                return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMessageAllFieldsEmpty) : result;
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No forex record found for {currency1}, {currency2}", ex);
        }
    }
}

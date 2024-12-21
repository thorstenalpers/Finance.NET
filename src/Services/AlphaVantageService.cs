using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Extensions;
using Finance.Net.Interfaces;
using Finance.Net.Models.AlphaVantage;
using Finance.Net.Models.AlphaVantage.Dtos;
using Finance.Net.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;

namespace Finance.Net.Services;

/// <inheritdoc />
public class AlphaVantageService(ILogger<AlphaVantageService> logger,
  IHttpClientFactory httpClientFactory,
  IOptions<FinanceNetConfiguration> options,
  IReadOnlyPolicyRegistry<string> policyRegistry) : IAlphaVantageService
{
    private readonly ILogger<AlphaVantageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly FinanceNetConfiguration _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private static ServiceProvider? s_serviceProvider;
    private readonly AsyncPolicy _retryPolicy = policyRegistry?.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy) ?? throw new ArgumentNullException(nameof(policyRegistry));

    /// <summary>
    /// Creates a service for interacting with the AlphaVantage API.
    /// Provides methods for retrieving company data, stock records, forex data, and intraday information.
    /// </summary>
    public static IAlphaVantageService Create()
    {
        return Create(new FinanceNetConfiguration());
    }

    /// <summary>
    /// Creates a service for interacting with the AlphaVantage API.
    /// Provides methods for retrieving company data, stock records, forex data, and intraday information.
    /// </summary>
    /// <param name="cfg">Configure .Net Finance. <see cref="FinanceNetConfiguration"/> ></param>
    public static IAlphaVantageService Create(FinanceNetConfiguration cfg)
    {
        if (s_serviceProvider == null)
        {
            var services = new ServiceCollection();
            services.AddFinanceNet(cfg);
            s_serviceProvider = services.BuildServiceProvider();
        }
        return s_serviceProvider.GetRequiredService<IAlphaVantageService>();
    }

    /// <inheritdoc />
    public async Task<Profile?> GetProfileAsync(string symbol, CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.AlphaVantageHttpClientName);
        var url = Constants.AlphaVantageApiUrl + "/query?function=OVERVIEW" +
            $"&symbol={symbol}" +
            $"&apikey={_options.AlphaVantageApiKey}";

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var httpResponse = await httpClient.GetAsync(url, token).ConfigureAwait(false);
                httpResponse.EnsureSuccessStatusCode();

                var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                if (jsonResponse.Contains(Constants.YahooResponseApiLimitExceeded))
                {
                    throw new FinanceNetException($"higher API call volume for {symbol}");
                }
                else
                {
                    var overview = JsonConvert.DeserializeObject<Profile>(jsonResponse);
                    var isNullObj = Helper.AreAllPropertiesNull(overview);
                    return isNullObj ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : overview;
                }
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No company overview found for {symbol}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Record>> GetRecordsAsync(string symbol, DateTime startDate, DateTime? endDate = null, CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.AlphaVantageHttpClientName);
        var url = Constants.AlphaVantageApiUrl + "/query?function=TIME_SERIES_DAILY_ADJUSTED" +
            $"&symbol={symbol}&outputsize=full&apikey={_options.AlphaVantageApiKey}";
        Guard.Against.NullOrEmpty(symbol);
        if (startDate > endDate)
        {
            throw new FinanceNetException("startDate earlier than endDate");
        }
        endDate ??= DateTime.UtcNow;
        if (endDate.Value.Date >= DateTime.UtcNow.Date)
        {
            endDate = DateTime.UtcNow.Date;
        }
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var result = new List<Record>();

                var jsonResponse = await Helper.FetchJsonDocumentAsync(httpClient, _logger, url, token);
                if (jsonResponse.Contains(Constants.YahooResponseApiLimitExceeded))
                {
                    throw new FinanceNetException($"higher API call volume for {symbol}");
                }
                ParseHistoryRecords(symbol, startDate, endDate, result, jsonResponse);
                return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No HistoryRecord found for {symbol}", ex);
        }
    }

    private void ParseHistoryRecords(string symbol, DateTime startDate, DateTime? endDate, List<Record> result, string jsonResponse)
    {
        var data = JsonConvert.DeserializeObject<DailyRecordRoot>(jsonResponse);
        if (data?.TimeSeries == null)
        {
            throw new FinanceNetException($"no daily records for {symbol}");
        }
        foreach (var item in data.TimeSeries)
        {
            var today = item.Key.Date;
            if (today > endDate || today < startDate || today > endDate)
            {
                continue;
            }
            if (result.Any(e => e.Date == today))
            {
                _logger.LogWarning("Bug: Course for {Symbol} for {Date} already added!", symbol, today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            else
            {
                result.Add(new Record
                {
                    Date = today,
                    Open = item.Value.Open,
                    Low = item.Value.Low,
                    High = item.Value.High,
                    Close = item.Value.Close,
                    AdjustedClose = item.Value.AdjustedClose,
                    Volume = item.Value.Volume,
                    SplitCoefficient = item.Value.SplitCoefficient,
                });
            }
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
            var currentCourses = await GetIntradayRecordsByMonthAsync(symbol, currentMonth, interval, token);
            result.AddRange(currentCourses);
        }
        return result.IsNullOrEmpty()
            ? throw new FinanceNetException($"Fail for {symbol}")
            : result.Where(e => e.DateTime >= startDate).ToList();
    }

    private async Task<List<IntradayRecord>> GetIntradayRecordsByMonthAsync(string symbol, DateTime month, EInterval interval, CancellationToken token = default)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.AlphaVantageHttpClientName);
        var url = Constants.AlphaVantageApiUrl + "/query?function=TIME_SERIES_INTRADAY" +
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
                var result = new List<IntradayRecord>();
                var response = await httpClient.GetAsync(url, token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                if (jsonResponse.Contains(Constants.YahooResponseApiLimitExceeded))
                {
                    throw new FinanceNetException($"higher API call volume for {symbol}");
                }
                ParseHistoryIntradayRecords(symbol, interval, result, jsonResponse);
                return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No HistoryIntradayRecord found for {symbol}", ex);
        }
    }

    private static void ParseHistoryIntradayRecords(string symbol, EInterval interval, List<IntradayRecord> result, string jsonResponse)
    {
        var data = JsonConvert.DeserializeObject<IntradayRecordRoot>(jsonResponse);

        var timeseries = interval switch
        {
            EInterval.Interval_1Min => data?.TimeSeries1Min ?? throw new FinanceNetException($"no intraday records for {symbol}"),
            EInterval.Interval_5Min => data?.TimeSeries5Min ?? throw new FinanceNetException($"no intraday records for {symbol}"),
            EInterval.Interval_15Min => data?.TimeSeries15Min ?? throw new FinanceNetException($"no intraday records for {symbol}"),
            EInterval.Interval_30Min => data?.TimeSeries30Min ?? throw new FinanceNetException($"no intraday records for {symbol}"),
            EInterval.Interval_60Min => data?.TimeSeries60Min ?? throw new FinanceNetException($"no intraday records for {symbol}"),
            _ => throw new NotImplementedException(),
        };
        foreach (var item in timeseries)
        {
            var dateTimeString = item.Key;
            var dateTimeSplit = dateTimeString.Split(' ');

            var date = dateTimeSplit[0];
            var time = dateTimeSplit[1];

            var dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            result.Add(new IntradayRecord
            {
                DateOnly = date,
                DateTime = dateTime,
                TimeOnly = time,

                Open = item.Value.Open,
                Low = item.Value.Low,
                High = item.Value.High,
                Close = item.Value.Close,
                Volume = item.Value.Volume,
            });
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
        var url = Constants.AlphaVantageApiUrl + "/query?function=FX_DAILY" +
            $"&from_symbol={currency1}&to_symbol={currency2}&outputsize=full&apikey={_options.AlphaVantageApiKey}";

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var result = new List<ForexRecord>();

                var jsonResponse = await Helper.FetchJsonDocumentAsync(httpClient, _logger, url, token);
                if (jsonResponse.Contains(Constants.YahooResponseApiLimitExceeded))
                {
                    throw new FinanceNetException($"higher API call volume for {currency1} /{currency2}");
                }
                ParseHistoryForexRecords(currency1, currency2, startDate, endDate, result, jsonResponse);
                return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
            });
        }
        catch (Exception ex)
        {
            throw new FinanceNetException($"No HistoryForexRecord found for {currency1}, {currency2}", ex);
        }
    }

    private void ParseHistoryForexRecords(string currency1, string currency2, DateTime startDate, DateTime? endDate, List<ForexRecord> result, string jsonResponse)
    {
        var data = JsonConvert.DeserializeObject<DailyForexRecordRoot>(jsonResponse);
        if (data?.TimeSeries == null)
        {
            throw new FinanceNetException($"no forex records for {currency1} /{currency2}");
        }
        foreach (var item in data.TimeSeries)
        {
            var today = item.Key;
            if (today > endDate || today < startDate || today > endDate)
            {
                continue;
            }
            if (result.Any(e => e.Date == today))
            {
                _logger.LogWarning("Bug: {Currency1} /{Currency2} for {Date} already added!", currency1, currency2, today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            else
            {
                result.Add(new ForexRecord
                {
                    Date = item.Key,
                    Open = item.Value.Open,
                    Low = item.Value.Low,
                    High = item.Value.High,
                    Close = item.Value.Close,
                });
            }
        }
    }
}

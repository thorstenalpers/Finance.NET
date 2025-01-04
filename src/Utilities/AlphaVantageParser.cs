using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ardalis.GuardClauses;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Models.AlphaVantage;
using Finance.Net.Models.AlphaVantage.Dtos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Finance.Net.Utilities;

/// <inheritdoc />
internal static class AlphaVantageParser
{
    public static List<IntradayRecord> ParseIntradayRecords(string symbol, EInterval interval, string jsonResponse)
    {
        var result = new List<IntradayRecord>();
        var data = JsonConvert.DeserializeObject<IntradayRecordRoot>(jsonResponse);

        var timeseries = interval switch
        {
            EInterval.Interval_1Min => data?.TimeSeries1Min ?? throw new FinanceNetException($"no timeSeries for {symbol}"),
            EInterval.Interval_5Min => data?.TimeSeries5Min ?? throw new FinanceNetException($"no timeSeries for {symbol}"),
            EInterval.Interval_15Min => data?.TimeSeries15Min ?? throw new FinanceNetException($"no timeSeries for {symbol}"),
            EInterval.Interval_30Min => data?.TimeSeries30Min ?? throw new FinanceNetException($"no timeSeries for {symbol}"),
            EInterval.Interval_60Min => data?.TimeSeries60Min ?? throw new FinanceNetException($"no timeSeries for {symbol}"),
            _ => throw new NotImplementedException(),
        };
        foreach (var item in timeseries)
        {
            var dateTime = DateTime.ParseExact(item.Key, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            result.Add(new IntradayRecord
            {
                DateTime = dateTime,

                Open = item.Value.Open,
                Low = item.Value.Low,
                High = item.Value.High,
                Close = item.Value.Close,
                Volume = item.Value.Volume,
            });
        }
        return result;
    }

    public static List<Record> ParseRecords<T>(string symbol, DateTime? startDate, DateTime? endDate, string jsonResponse, ILogger<T> logger)
    {
        var result = new List<Record>();
        Guard.Against.Null(startDate);
        var data = JsonConvert.DeserializeObject<DailyRecordRoot>(jsonResponse);
        if (data?.TimeSeries == null)
        {
            throw new FinanceNetException("data is invalid");
        }
        foreach (var item in data.TimeSeries)
        {
            var today = item.Key.Date;
            if (today > endDate || today < startDate)
            {
                continue;
            }
            if (result.Any(e => e.Date == today))
            {
                logger.LogWarning("Bug: Course for {Symbol} for {Date} already added!", symbol, today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
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
        return result;
    }

    public static List<ForexRecord> ParseForexRecords<T>(string currency1, string currency2, DateTime startDate, DateTime? endDate, string jsonResponse, ILogger<T> logger)
    {
        var result = new List<ForexRecord>();
        var data = JsonConvert.DeserializeObject<DailyForexRecordRoot>(jsonResponse);
        if (data?.TimeSeries == null)
        {
            throw new FinanceNetException("data is invalid");
        }
        foreach (var item in data.TimeSeries)
        {
            var today = item.Key;
            if (today > endDate || today < startDate)
            {
                continue;
            }
            if (result.Any(e => e.Date == today))
            {
                logger.LogWarning("Bug: {Currency1} /{Currency2} for {Date} already added!", currency1, currency2, today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
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
        return result;
    }
}
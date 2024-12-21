using System;

namespace Finance.Net.Models.AlphaVantage;

/// <summary>
/// Represents an intraday record of stock market data, including price and volume information for a specific date and time.
/// </summary>
public record IntradayRecord
{
    /// <summary>
    /// The date and time of the record.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// The date part of the record in string format (e.g., "yyyy-MM-dd").
    /// </summary>
    public string? DateOnly { get; set; }

    /// <summary>
    /// The time part of the record in string format (e.g., "HH:mm:ss").
    /// </summary>
    public string? TimeOnly { get; set; }

    /// <summary>
    /// The opening price of the asset for the given date and time.
    /// </summary>
    public double Open { get; set; }

    /// <summary>
    /// The highest price of the asset during the given time period.
    /// </summary>
    public double High { get; set; }

    /// <summary>
    /// The lowest price of the asset during the given time period.
    /// </summary>
    public double Low { get; set; }

    /// <summary>
    /// The closing price of the asset for the given time period.
    /// </summary>
    public double Close { get; set; }

    /// <summary>
    /// The trading volume for the asset during the given time period.
    /// </summary>
    public long Volume { get; set; }
}

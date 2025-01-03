using System;

namespace Finance.Net.Models.AlphaVantage;

/// <summary>
/// Represents an intraday record of stock market data.
/// </summary>
public record IntradayRecord
{
    /// <summary>
    /// The date and time of the record.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// The opening price.
    /// </summary>
    public double Open { get; set; }

    /// <summary>
    /// The highest price.
    /// </summary>
    public double High { get; set; }

    /// <summary>
    /// The lowest price.
    /// </summary>
    public double Low { get; set; }

    /// <summary>
    /// The closing price.
    /// </summary>
    public double Close { get; set; }

    /// <summary>
    /// The trading volume.
    /// </summary>
    public long Volume { get; set; }
}

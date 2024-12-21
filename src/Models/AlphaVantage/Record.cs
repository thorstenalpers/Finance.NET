using System;

namespace Finance.Net.Models.AlphaVantage;
/// <summary>
/// Represents a historical record of stock market data, including price, volume, and split coefficient information for a specific date.
/// </summary>
public record Record
{
    /// <summary>
    /// The date of the record.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The opening price of the asset for the given date.
    /// </summary>
    public double? Open { get; set; }

    /// <summary>
    /// The lowest price of the asset during the given day.
    /// </summary>
    public double? Low { get; set; }

    /// <summary>
    /// The highest price of the asset during the given day.
    /// </summary>
    public double? High { get; set; }

    /// <summary>
    /// The closing price of the asset for the given date.
    /// </summary>
    public double? Close { get; set; }

    /// <summary>
    /// The adjusted closing price of the asset, accounting for stock splits and dividends.
    /// </summary>
    public double? AdjustedClose { get; set; }

    /// <summary>
    /// The trading volume for the asset during the given date.
    /// </summary>
    public long? Volume { get; set; }

    /// <summary>
    /// The stock split coefficient for the given date, if applicable.
    /// </summary>
    public double? SplitCoefficient { get; set; }
}

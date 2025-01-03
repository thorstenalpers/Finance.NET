using System;

namespace Finance.Net.Models.Yahoo;

/// <summary>
/// Represents a historical record of stock market data.
/// </summary>
public record Record
{
    /// <summary>
    /// The date of the record.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The opening pricee.
    /// </summary>
    public decimal? Open { get; set; }

    /// <summary>
    /// The highest price.
    /// </summary>
    public decimal? High { get; set; }

    /// <summary>
    /// The lowest price.
    /// </summary>
    public decimal? Low { get; set; }

    /// <summary>
    /// The closing price.
    /// </summary>
    public decimal? Close { get; set; }

    /// <summary>
    /// The adjusted closing price, accounting for stock splits and dividends.
    /// </summary>
    public decimal? AdjustedClose { get; set; }

    /// <summary>
    /// The trading volume.
    /// </summary>
    public long? Volume { get; set; }
}


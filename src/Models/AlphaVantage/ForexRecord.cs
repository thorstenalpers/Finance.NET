using System;

namespace Finance.Net.Models.AlphaVantage;

/// <summary>
/// Represents a historical record of forex data, including open, high, low, and close prices, as well as the date of the record.
/// </summary>
public record ForexRecord
{
    /// <summary>
    /// The date of the forex record.
    /// </summary>
    /// <value>The date of the record, or <c>null</c> if not available.</value>
    public DateTime? Date { get; set; }

    /// <summary>
    /// The opening price of the forex pair on the given date.
    /// </summary>
    /// <value>The opening price, or <c>null</c> if not available.</value>
    public double? Open { get; set; }

    /// <summary>
    /// The highest price of the forex pair on the given date.
    /// </summary>
    /// <value>The highest price, or <c>null</c> if not available.</value>
    public double? High { get; set; }

    /// <summary>
    /// The lowest price of the forex pair on the given date.
    /// </summary>
    /// <value>The lowest price, or <c>null</c> if not available.</value>
    public double? Low { get; set; }

    /// <summary>
    /// The closing price of the forex pair on the given date.
    /// </summary>
    /// <value>The closing price, or <c>null</c> if not available.</value>
    public double? Close { get; set; }
}

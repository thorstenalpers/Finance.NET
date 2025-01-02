using System;

namespace Finance.Net.Models.AlphaVantage;

/// <summary>
/// Represents a historical record of forex data.
/// </summary>
public record ForexRecord
{
    /// <summary>
    /// The date of the forex record.
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// The opening price.
    /// </summary>
    public double? Open { get; set; }

    /// <summary>
    /// The highest price.
    /// </summary>
    public double? High { get; set; }

    /// <summary>
    /// The lowest price.
    /// </summary>
    public double? Low { get; set; }

    /// <summary>
    /// The closing price.
    /// </summary>
    public double? Close { get; set; }
}

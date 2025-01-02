using System.ComponentModel;

namespace Finance.Net.Enums;

/// <summary>
/// Represents the different time intervals for financial data or market analysis.
/// </summary>
public enum EInterval
{
    /// <summary>
    /// The 1-minute interval.
    /// </summary>
    [Description("1min")]
    Interval_1Min = 1,

    /// <summary>
    /// The 5-minute interval.
    /// </summary>
    [Description("5min")]
    Interval_5Min = 2,

    /// <summary>
    /// The 15-minute interval.
    /// </summary>
    [Description("15min")]
    Interval_15Min = 3,

    /// <summary>
    /// The 30-minute interval.
    /// </summary>
    [Description("30min")]
    Interval_30Min = 4,

    /// <summary>
    /// The 60-minute interval.
    /// </summary>
    [Description("60min")]
    Interval_60Min = 5
}

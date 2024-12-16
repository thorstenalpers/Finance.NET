using System;

namespace Finance.Net.Models.AlphaVantage;
public record HistoryIntradayRecord
{
    public DateTime DateTime { get; set; }

    public string? DateOnly { get; set; }

    public string? TimeOnly { get; set; }

    public double Open { get; set; }

    public double High { get; set; }

    public double Low { get; set; }

    public double Close { get; set; }

    public long Volume { get; set; }
}
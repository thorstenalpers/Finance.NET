using System;

namespace Finance.Net.Models.Yahoo;

public record HistoryRecord
{
    public DateTime Date { get; set; }

    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? Close { get; set; }
    public decimal? AdjustedClose { get; set; }
    public long? Volume { get; set; }
}

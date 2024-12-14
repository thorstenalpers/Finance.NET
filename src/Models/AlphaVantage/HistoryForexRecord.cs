using System;

namespace Finance.Net.Models.AlphaVantage;

public record HistoryForexRecord
{
	public DateTime? Date { get; set; }
	public double? Open { get; set; }

	public double? High { get; set; }

	public double? Low { get; set; }

	public double? Close { get; set; }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Finance.Net.Models.AlphaVantage.Dtos;

internal record DailyRecordRoot
{
	[JsonProperty("Meta Data")]
	public Dictionary<string, string>? MetaData { get; set; }

	[JsonProperty("Time Series (Daily)")]
	public Dictionary<DateTime, DailyRecordItem>? TimeSeries { get; set; }
}

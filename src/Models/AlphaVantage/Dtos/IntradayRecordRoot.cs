
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Finance.Net.Models.AlphaVantage.Dtos;

internal record IntradayRecordRoot
{
	[JsonProperty("Meta Data")]
	public Dictionary<string, string>? MetaData { get; set; }

	[JsonProperty("Time Series (1min)")]
	public Dictionary<string, IntradayRecordItem>? TimeSeries1Min { get; set; }

	[JsonProperty("Time Series (5min)")]
	public Dictionary<string, IntradayRecordItem>? TimeSeries5Min { get; set; }

	[JsonProperty("Time Series (15min)")]
	public Dictionary<string, IntradayRecordItem>? TimeSeries15Min { get; set; }

	[JsonProperty("Time Series (30min)")]
	public Dictionary<string, IntradayRecordItem>? TimeSeries30Min { get; set; }

	[JsonProperty("Time Series (60min)")]
	public Dictionary<string, IntradayRecordItem>? TimeSeries60Min { get; set; }
}


using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Finance.Net.Models.AlphaVantage.Dtos;

internal record DailyForexRecordRoot
{
		[JsonProperty("Meta Data")]
		public Dictionary<string, string>? MetaData { get; set; }

		[JsonProperty("Time Series FX (Daily)")]
		public Dictionary<DateTime, DailyForexRecordItem>? TimeSeries { get; set; }
}

using Newtonsoft.Json;

namespace NetFinance.Application.Models;

public record Security
{
	[JsonProperty("language")]
	public string? Language { get; set; }

	[JsonProperty("region")]
	public string? Region { get; set; }

	[JsonProperty("quoteType")]
	public string? QuoteType { get; set; }

	[JsonProperty("typeDisp")]
	public string? TypeDisp { get; set; }

	[JsonProperty("quoteSourceName")]
	public string? QuoteSourceName { get; set; }

	[JsonProperty("triggerable")]
	public bool? Triggerable { get; set; }

	[JsonProperty("customPriceAlertConfidence")]
	public string? CustomPriceAlertConfidence { get; set; }

	[JsonProperty("marketState")]
	public string? MarketState { get; set; }

	[JsonProperty("exchange")]
	public string? Exchange { get; set; }

	[JsonProperty("exchangeTimezoneName")]
	public string? ExchangeTimezoneName { get; set; }

	[JsonProperty("exchangeTimezoneShortName")]
	public string? ExchangeTimezoneShortName { get; set; }

	[JsonProperty("gmtOffSetMilliseconds")]
	public long? GmtOffSetMilliseconds { get; set; }

	[JsonProperty("market")]
	public string? Market { get; set; }

	[JsonProperty("esgPopulated")]
	public bool? EsgPopulated { get; set; }

	[JsonProperty("regularMarketPrice")]
	public double? RegularMarketPrice { get; set; }

	[JsonProperty("regularMarketTime")]
	public long? RegularMarketTime { get; set; }

	[JsonProperty("fullExchangeName")]
	public string? FullExchangeName { get; set; }

	[JsonProperty("sourceInterval")]
	public long? SourceInterval { get; set; }

	[JsonProperty("exchangeDataDelayedBy")]
	public long? ExchangeDataDelayedBy { get; set; }

	[JsonProperty("tradeable")]
	public bool? Tradeable { get; set; }

	[JsonProperty("cryptoTradeable")]
	public bool? CryptoTradeable { get; set; }

	[JsonProperty("hasPrePostMarketData")]
	public bool? HasPrePostMarketData { get; set; }

	[JsonProperty("firstTradeDateMilliseconds")]
	public long? FirstTradeDateMilliseconds { get; set; }

	[JsonProperty("priceHint")]
	public long? PriceHint { get; set; }

	[JsonProperty("symbol")]
	public string? Symbol { get; set; }
}
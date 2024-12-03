using Newtonsoft.Json;

namespace NetFinance.Application.Models;

internal record QuoteResponseRoot
{
	[JsonProperty("quoteResponse")]
	public QuoteResponse? QuoteResponse { get; set; }
}
using Newtonsoft.Json;

namespace Finance.Net.Models.Yahoo.Dtos;

internal record QuoteResponseRoot
{
    [JsonProperty("quoteResponse")]
    public QuoteResponseSummary? QuoteResponse { get; set; }
}
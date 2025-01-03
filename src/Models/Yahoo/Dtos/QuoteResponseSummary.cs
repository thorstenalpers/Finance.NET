using Newtonsoft.Json;

namespace Finance.Net.Models.Yahoo.Dtos;

internal record QuoteResponseSummary
{
    [JsonProperty("result")]
    public QuoteResponse[]? Result { get; set; }

    [JsonProperty("error")]
    public object? Error { get; set; }
}
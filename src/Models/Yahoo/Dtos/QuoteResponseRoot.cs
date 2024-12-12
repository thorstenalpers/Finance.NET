using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DotNetFinance.Models.Yahoo.Dtos;

[ExcludeFromCodeCoverage]
internal record QuoteResponseRoot
{
	[JsonProperty("quoteResponse")]
	public QuoteResponseSummary? QuoteResponse { get; set; }
}
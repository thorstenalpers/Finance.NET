using System.ComponentModel.DataAnnotations;

namespace Finance.Net;

/// <summary>
/// Configure Finance.NET
/// </summary>
public class FinanceNetConfiguration
{
    /// <summary> Default retries for failed http requests (caused by rate limits), default 10 retries </summary>
    [Required] public int HttpRetryCount { get; set; } = 10;

    /// <summary> Default HTTP timeout in seconds, default 20 seconds </summary>
    [Required] public int HttpTimeout { get; set; } = 20;

    /// <summary> Alpha Vantage API Key, default null </summary>
    public string? AlphaVantageApiKey { get; set; }
}

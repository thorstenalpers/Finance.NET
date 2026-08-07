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

    /// <summary>
    /// Base wait between retries in seconds, default 5 seconds. Retries back off
    /// exponentially from this base (5s, 10s, 20s, ...) with jitter, capped at 30 seconds
    /// per attempt. Set to 0 to retry without waiting.
    /// </summary>
    [Required] public int HttpRetrySleepTime { get; set; } = 5;

    /// <summary> Alpha Vantage API Key, default null </summary>
    public string? AlphaVantageApiKey { get; set; }
}

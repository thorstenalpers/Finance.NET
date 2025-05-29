using System;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Finance.Net.Utilities;

internal static class PollyPolicyFactory
{
    public static AsyncRetryPolicy GetRetryPolicy<T>(int retryCount, int waitTimeSecs, ILogger<T>? logger)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(waitTimeSecs * retryAttempt), // delayed retry, 1,2,3,..secs
                (exception, timeSpan, retryCount, _) =>
                {
                    logger?.LogWarning("Retry {RetryCount} after {TimeSpan} due to {Exception}.", retryCount, timeSpan, exception?.Message);
                });
    }
}

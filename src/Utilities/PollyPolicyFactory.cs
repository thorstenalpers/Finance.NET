using System;
using System.Security.Cryptography;
using Finance.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Finance.Net.Utilities;

internal static class PollyPolicyFactory
{
    /// <summary>Upper bound for a single back-off, so a high retry count cannot run away.</summary>
    internal const int MaxRetryDelaySecs = 30;

    public static AsyncRetryPolicy GetRetryPolicy<T>(int retryCount, int baseWaitTimeSecs, ILogger<T> logger)
    {
        return Policy
            // A provider that answered with no data has given a permanent answer - retrying
            // it burns the whole back-off budget and cannot change the outcome.
            .Handle<Exception>(ex => ex is not FinanceNetNoDataException)
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => GetRetryDelay(retryAttempt, baseWaitTimeSecs),
                (exception, timeSpan, retryCount, _) =>
                {
                    logger?.LogWarning("Retry {RetryCount} after {TimeSpan} due to {Exception}.", retryCount, timeSpan, exception?.Message);
                });
    }

    /// <summary>
    /// Exponential back-off from <paramref name="baseWaitTimeSecs"/> (1x, 2x, 4x, ...), capped at
    /// <see cref="MaxRetryDelaySecs"/>, plus up to one full delay of jitter so concurrent
    /// callers do not retry in lockstep.
    /// </summary>
    internal static TimeSpan GetRetryDelay(int retryAttempt, int baseWaitTimeSecs)
    {
        if (baseWaitTimeSecs <= 0)
        {
            return TimeSpan.Zero;
        }
        var exponent = Math.Min(retryAttempt - 1, 30);  // keep Pow away from infinity
        var backOffSecs = Math.Min(baseWaitTimeSecs * Math.Pow(2, exponent), MaxRetryDelaySecs);
        // Jitter scales with the delay, not with the base: once the cap is reached a
        // base-sized window would put every caller back into lockstep.
        var jitterMs = RandomNumberGenerator.GetInt32(0, (int)(backOffSecs * 1000));
        return TimeSpan.FromSeconds(backOffSecs) + TimeSpan.FromMilliseconds(jitterMs);
    }
}

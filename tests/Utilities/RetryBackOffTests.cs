using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
using Finance.Net.Extensions;
using Finance.Net.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Polly;
using Polly.Registry;

namespace Finance.Net.Tests.Utilities;

/// <summary>
/// The retry back-off is its own configuration knob. It used to reuse
/// <see cref="FinanceNetConfiguration.HttpTimeout"/> as its base, so raising the HTTP
/// timeout silently multiplied total retry latency.
/// </summary>
[TestFixture]
[Category("Unit")]
public class RetryBackOffTests
{
    [Test]
    public void HttpRetrySleepTime_DefaultsToOneSecond()
    {
        Assert.That(new FinanceNetConfiguration().HttpRetrySleepTime, Is.EqualTo(1));
    }

    [Test]
    public void GetRetryDelay_GrowsExponentially()
    {
        var delays = Enumerable.Range(1, 4)
            .Select(attempt => PollyPolicyFactory.GetRetryDelay(attempt, 1).TotalSeconds)
            .ToList();

        // 1s, 2s, 4s, 8s, each plus up to one base interval of jitter.
        Assert.That(delays[0], Is.InRange(1.0, 2.0));
        Assert.That(delays[1], Is.InRange(2.0, 3.0));
        Assert.That(delays[2], Is.InRange(4.0, 5.0));
        Assert.That(delays[3], Is.InRange(8.0, 9.0));
    }

    [Test]
    public void GetRetryDelay_IsCapped()
    {
        var delay = PollyPolicyFactory.GetRetryDelay(20, 1);

        Assert.That(delay.TotalSeconds,
            Is.InRange(PollyPolicyFactory.MaxRetryDelaySecs, PollyPolicyFactory.MaxRetryDelaySecs + 1.0));
    }

    [Test]
    public void GetRetryDelay_AddsJitter()
    {
        var delays = Enumerable.Range(0, 30)
            .Select(_ => PollyPolicyFactory.GetRetryDelay(1, 4))
            .Distinct()
            .ToList();

        Assert.That(delays.Count, Is.GreaterThan(1), "back-off is not jittered - retries stay in lockstep");
    }

    [Test]
    public void GetRetryDelay_ZeroBase_IsZero()
    {
        Assert.That(PollyPolicyFactory.GetRetryDelay(3, 0), Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void RegisteredPolicy_BacksOffByHttpRetrySleepTime_NotHttpTimeout()
    {
        var services = new ServiceCollection();
        services.AddFinanceNet(new FinanceNetConfiguration
        {
            HttpRetryCount = 2,
            HttpTimeout = 30,          // generous transport timeout ...
            HttpRetrySleepTime = 0,    // ... must not become a 30s + 60s back-off
        });
        var provider = services.BuildServiceProvider();
        var policy = provider.GetRequiredService<IReadOnlyPolicyRegistry<string>>()
                             .Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy);

        var stopwatch = Stopwatch.StartNew();
        Assert.ThrowsAsync<FinanceNetException>(
            async () => await policy.ExecuteAsync<int>(() => throw new FinanceNetException("transient")));
        stopwatch.Stop();

        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(2)),
            $"retries slept for {stopwatch.ElapsedMilliseconds}ms - HttpTimeout is still driving the back-off");
    }

    [Test]
    public void AddFinanceNet_CarriesHttpRetrySleepTime()
    {
        var services = new ServiceCollection();
        services.AddFinanceNet(new FinanceNetConfiguration { HttpRetrySleepTime = 7 });
        var provider = services.BuildServiceProvider();

        var resolved = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<FinanceNetConfiguration>>().Value;
        Assert.That(resolved.HttpRetrySleepTime, Is.EqualTo(7));
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Interfaces;
using Finance.Net.Services;
using Finance.Net.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Polly;
using Polly.Registry;

namespace Finance.Net.Tests.Services;

/// <summary>
/// The retry policy sleeps between attempts. Those sleeps must observe the caller's
/// <see cref="CancellationToken"/>, otherwise a cancelled call keeps running for the full
/// back-off budget. Each test cancels shortly after the call starts and asserts the call
/// gives up well before the policy would have finished sleeping.
/// </summary>
[TestFixture]
[Category("Unit")]
public class CancellationTokenPropagationTests
{
    private const int RetryCount = 3;
    private static readonly TimeSpan RetrySleep = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan CancelAfter = TimeSpan.FromMilliseconds(150);

    /// <summary>Comfortably below the first back-off sleep, comfortably above the cancel delay.</summary>
    private static readonly TimeSpan MaxAcceptable = TimeSpan.FromMilliseconds(1500);

    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;

    [SetUp]
    public void SetUp()
    {
        // Always fails, so the policy always reaches its back-off sleep.
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("transient"));

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockHandler.Object));

        AsyncPolicy policy = Policy.Handle<Exception>().WaitAndRetryAsync(RetryCount, _ => RetrySleep);
        IAsyncPolicy asyncPolicy = policy;

        _mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
        _mockPolicyRegistry.Setup(registry => registry.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy)).Returns(policy);
        _mockPolicyRegistry.Setup(registry => registry.Get<IAsyncPolicy>(Constants.DefaultHttpRetryPolicy)).Returns(policy);
        _mockPolicyRegistry.Setup(registry => registry.TryGet(Constants.DefaultHttpRetryPolicy, out asyncPolicy)).Returns(true);
    }

    [Test]
    public void GetQuotesAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var mockSession = new Mock<IYahooSessionManager>();
        var service = new YahooFinanceService(
            Mock.Of<ILogger<YahooFinanceService>>(),
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            mockSession.Object);

        AssertCancelsPromptly(token => service.GetQuotesAsync(["IBM"], token));
    }

    [Test]
    public void GetRecordsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var mockSession = new Mock<IYahooSessionManager>();
        var service = new YahooFinanceService(
            Mock.Of<ILogger<YahooFinanceService>>(),
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            mockSession.Object);

        AssertCancelsPromptly(token => service.GetRecordsAsync("IBM", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, token));
    }

    [Test]
    public void AlphaVantage_GetRecordsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var mockOptions = new Mock<IOptions<FinanceNetConfiguration>>();
        mockOptions.Setup(x => x.Value).Returns(new FinanceNetConfiguration());
        var service = new AlphaVantageService(
            Mock.Of<ILogger<AlphaVantageService>>(),
            _mockHttpClientFactory.Object,
            mockOptions.Object,
            _mockPolicyRegistry.Object);

        AssertCancelsPromptly(token => service.GetRecordsAsync("IBM", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, token));
    }

    [Test]
    public void Xetra_GetInstrumentsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = new XetraService(
            Mock.Of<ILogger<XetraService>>(),
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object);

        AssertCancelsPromptly(service.GetInstrumentsAsync);
    }

    [Test]
    public void DataHub_GetNasdaqInstrumentsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = new DataHubService(_mockHttpClientFactory.Object, _mockPolicyRegistry.Object);

        AssertCancelsPromptly(service.GetNasdaqInstrumentsAsync);
    }

    [Test]
    public void RefreshSessionAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var mockState = new Mock<IYahooSessionState>();
        mockState.Setup(e => e.IsValid()).Returns(false);
        mockState.Setup(e => e.GetCookieContainer()).Returns(new System.Net.CookieContainer());
        var manager = new YahooSessionManager(
            Mock.Of<ILogger<YahooSessionManager>>(),
            _mockHttpClientFactory.Object,
            mockState.Object,
            _mockPolicyRegistry.Object);

        AssertCancelsPromptly(manager.RefreshSessionAsync);
    }

    private static void AssertCancelsPromptly(Func<CancellationToken, Task> call)
    {
        using var cts = new CancellationTokenSource(CancelAfter);
        var stopwatch = Stopwatch.StartNew();

        Assert.CatchAsync<OperationCanceledException>(async () => await call(cts.Token));

        stopwatch.Stop();
        Assert.That(stopwatch.Elapsed, Is.LessThan(MaxAcceptable),
            $"call kept running for {stopwatch.ElapsedMilliseconds}ms after cancellation - the token never reached the retry back-off");
    }
}

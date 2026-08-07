using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Enums;
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

    // Below the 2s back-off sleep, but with enough headroom for a CI runner under coverage
    // instrumentation - the exception type already separates fixed from broken on its own.
    private static readonly TimeSpan MaxAcceptable = TimeSpan.FromMilliseconds(1900);

    private static readonly DateTime StartDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
    public void GetInstrumentsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var mockSession = new Mock<IYahooSessionManager>();
        var service = new YahooFinanceService(
            Mock.Of<ILogger<YahooFinanceService>>(),
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            mockSession.Object);

        // Unfiltered, so cancellation has to escape both the per-type log-and-continue loop
        // and the partial-result catch in FetchSymbolsAsync.
        AssertCancelsPromptly(token => service.GetInstrumentsAsync(null, token));
    }

    [Test]
    public void GetProfileAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = CreateYahooService();

        AssertCancelsPromptly(token => service.GetProfileAsync("IBM", token));
    }

    [Test]
    public void GetSummaryAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = CreateYahooService();

        AssertCancelsPromptly(token => service.GetSummaryAsync("IBM", token));
    }

    [Test]
    public void GetFinancialsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = CreateYahooService();

        AssertCancelsPromptly(token => service.GetFinancialsAsync("IBM", token));
    }

    [Test]
    public void AlphaVantage_GetRecordsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = CreateAlphaVantageService();

        AssertCancelsPromptly(token => service.GetRecordsAsync("IBM", StartDate, null, token));
    }

    [Test]
    public void AlphaVantage_GetOverviewAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = CreateAlphaVantageService();

        AssertCancelsPromptly(token => service.GetOverviewAsync("IBM", token));
    }

    [Test]
    public void AlphaVantage_GetIntradayRecordsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = CreateAlphaVantageService();

        AssertCancelsPromptly(token => service.GetIntradayRecordsAsync("IBM", StartDate, null, EInterval.Interval_15Min, token));
    }

    [Test]
    public void AlphaVantage_GetForexRecordsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = CreateAlphaVantageService();

        AssertCancelsPromptly(token => service.GetForexRecordsAsync("EUR", "USD", StartDate, null, token));
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
    public void DataHub_GetSp500InstrumentsAsync_Cancelled_StopsWithoutWaitingOutTheBackOff()
    {
        var service = new DataHubService(_mockHttpClientFactory.Object, _mockPolicyRegistry.Object);

        AssertCancelsPromptly(service.GetSp500InstrumentsAsync);
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

    private YahooFinanceService CreateYahooService() => new(
        Mock.Of<ILogger<YahooFinanceService>>(),
        _mockHttpClientFactory.Object,
        _mockPolicyRegistry.Object,
        Mock.Of<IYahooSessionManager>());

    private AlphaVantageService CreateAlphaVantageService()
    {
        var mockOptions = new Mock<IOptions<FinanceNetConfiguration>>();
        mockOptions.Setup(x => x.Value).Returns(new FinanceNetConfiguration());
        return new AlphaVantageService(
            Mock.Of<ILogger<AlphaVantageService>>(),
            _mockHttpClientFactory.Object,
            mockOptions.Object,
            _mockPolicyRegistry.Object);
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

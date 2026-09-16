using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
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
/// Alpha Vantage refusing a request for the account (premium-only endpoint, invalid API key)
/// is a permanent answer: retrying cannot change it. It must surface as
/// <see cref="FinanceNetAccessDeniedException"/> and cost exactly one request.
/// </summary>
[TestFixture]
[Category("Unit")]
public class AlphaVantageRejectedRequestTests
{
    private const int RetryCount = 3;
    private const string PremiumResponse =
        "{\"Information\": \"Thank you for using Alpha Vantage! This is a premium endpoint. You may subscribe to any of the premium plans at https://www.alphavantage.co/premium/ to instantly unlock all premium endpoints\"}";
    private const string InvalidApiKeyResponse =
        "{\"Error Message\": \"the parameter apikey is invalid or missing. Please claim your free API key on (https://www.alphavantage.co/support/#api-key). It should take less than 20 seconds.\"}";

    private int _requestCount;
    private AlphaVantageService _service;

    private void SetUpResponse(string body)
    {
        _requestCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                _requestCount++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                });
            });

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(mockHandler.Object));

        IAsyncPolicy policy = PollyPolicyFactory.GetRetryPolicy<AlphaVantageService>(RetryCount, 0, null);
        var mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
        mockPolicyRegistry.Setup(registry => registry.TryGet(Constants.DefaultHttpRetryPolicy, out policy)).Returns(true);

        var mockOptions = new Mock<IOptions<FinanceNetConfiguration>>();
        mockOptions.Setup(x => x.Value).Returns(new FinanceNetConfiguration());

        _service = new AlphaVantageService(
            Mock.Of<ILogger<AlphaVantageService>>(),
            mockHttpClientFactory.Object,
            mockOptions.Object,
            mockPolicyRegistry.Object);
    }

    private static readonly DateTime StartDate = new(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly object[] Calls =
    [
        new object[] { "GetRecordsAsync", (Func<AlphaVantageService, Task>)(s => s.GetRecordsAsync("IBM", StartDate)) },
        new object[] { "GetOverviewAsync", (Func<AlphaVantageService, Task>)(s => s.GetOverviewAsync("IBM")) },
        new object[] { "GetIntradayRecordsAsync", (Func<AlphaVantageService, Task>)(s => s.GetIntradayRecordsAsync("IBM", StartDate, StartDate.AddDays(1))) },
        new object[] { "GetForexRecordsAsync", (Func<AlphaVantageService, Task>)(s => s.GetForexRecordsAsync("EUR", "USD", StartDate)) },
    ];

    [TestCaseSource(nameof(Calls))]
    public void PremiumEndpoint_FailsFastWithoutRetrying(string name, Func<AlphaVantageService, Task> call)
    {
        SetUpResponse(PremiumResponse);

        var exception = Assert.CatchAsync<FinanceNetAccessDeniedException>(async () => await call(_service), name);
        Assert.That(exception.Message, Does.Contain("premium endpoint"), name);
        Assert.That(exception.InnerException, Is.Null, name);
        Assert.That(_requestCount, Is.EqualTo(1), $"{name}: a premium-endpoint refusal was retried");
    }

    [TestCaseSource(nameof(Calls))]
    public void InvalidApiKey_FailsFastWithoutRetrying(string name, Func<AlphaVantageService, Task> call)
    {
        SetUpResponse(InvalidApiKeyResponse);

        var exception = Assert.CatchAsync<FinanceNetAccessDeniedException>(async () => await call(_service), name);
        Assert.That(exception.Message, Does.Contain("apikey is invalid"), name);
        Assert.That(_requestCount, Is.EqualTo(1), $"{name}: an invalid-API-key refusal was retried");
    }

    [Test]
    public void ApiLimitExceeded_IsStillRetried()
    {
        // The per-minute limit clears on its own, so it stays a retryable failure.
        SetUpResponse("{\"Information\": \"Please consider spreading out your free API requests more sparingly (1 request per second). You may subscribe to any of the premium plans at https://www.alphavantage.co/premium/ to lift the free key rate limit, or visit higher API call volume.\"}");

        var exception = Assert.CatchAsync<FinanceNetException>(async () => await _service.GetRecordsAsync("IBM", StartDate));
        Assert.That(exception, Is.Not.InstanceOf<FinanceNetAccessDeniedException>());
        Assert.That(_requestCount, Is.EqualTo(RetryCount + 1));
    }

    [Test]
    public void FinanceNetAccessDeniedException_IsAFinanceNetException()
    {
        Assert.That(new FinanceNetAccessDeniedException("denied"), Is.InstanceOf<FinanceNetException>());
    }
}

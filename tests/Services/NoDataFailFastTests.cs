using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
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
/// A well-formed provider response that simply carries no data is a permanent answer:
/// retrying cannot change it. It must surface as <see cref="FinanceNetNoDataException"/>
/// and cost exactly one request, not one per retry.
/// </summary>
[TestFixture]
[Category("Unit")]
public class NoDataFailFastTests
{
    private const int RetryCount = 3;

    private int _requestCount;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;

    private void SetUpResponse(string body, string mediaType)
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
                    Content = new StringContent(body, Encoding.UTF8, mediaType),
                });
            });

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockHandler.Object));

        var policy = PollyPolicyFactory.GetRetryPolicy<YahooFinanceService>(RetryCount, 0, null);
        IAsyncPolicy asyncPolicy = policy;

        _mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
        _mockPolicyRegistry.Setup(registry => registry.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy)).Returns(policy);
        _mockPolicyRegistry.Setup(registry => registry.Get<IAsyncPolicy>(Constants.DefaultHttpRetryPolicy)).Returns(policy);
        _mockPolicyRegistry.Setup(registry => registry.TryGet(Constants.DefaultHttpRetryPolicy, out asyncPolicy)).Returns(true);
    }

    [Test]
    public void Yahoo_GetQuotesAsync_EmptyResultSet_FailsFastWithoutRetrying()
    {
        SetUpResponse("{\"quoteResponse\":{\"result\":[],\"error\":null}}", "application/json");
        var service = new YahooFinanceService(
            Mock.Of<ILogger<YahooFinanceService>>(),
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            Mock.Of<IYahooSessionManager>());

        Assert.CatchAsync<FinanceNetNoDataException>(async () => await service.GetQuotesAsync(["BOGUSTICKER"]));
        Assert.That(_requestCount, Is.EqualTo(1), "an empty result set was retried");
    }

    [Test]
    public void Yahoo_GetRecordsAsync_NoRecords_FailsFastWithoutRetrying()
    {
        // A real history page for a symbol with no bars in range: correct table, no rows.
        SetUpResponse(
            "<html><head><title>IBM</title></head><body><table class=\"table\"><thead><tr>" +
            "<th>Date</th><th>Open</th><th>High</th><th>Low</th><th>Close</th><th>Adj Close</th><th>Volume</th>" +
            "</tr></thead><tbody></tbody></table></body></html>",
            "text/html");
        var service = new YahooFinanceService(
            Mock.Of<ILogger<YahooFinanceService>>(),
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            Mock.Of<IYahooSessionManager>());

        Assert.CatchAsync<FinanceNetNoDataException>(
            async () => await service.GetRecordsAsync("BOGUSTICKER", new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)));
        Assert.That(_requestCount, Is.EqualTo(1), "an empty history page was retried");
    }

    [Test]
    public void AlphaVantage_GetRecordsAsync_NoRecords_FailsFastWithoutRetrying()
    {
        // A valid time series that happens to be empty.
        SetUpResponse("{\"Time Series (Daily)\":{}}", "application/json");
        var mockOptions = new Mock<IOptions<FinanceNetConfiguration>>();
        mockOptions.Setup(x => x.Value).Returns(new FinanceNetConfiguration());
        var service = new AlphaVantageService(
            Mock.Of<ILogger<AlphaVantageService>>(),
            _mockHttpClientFactory.Object,
            mockOptions.Object,
            _mockPolicyRegistry.Object);

        Assert.CatchAsync<FinanceNetNoDataException>(
            async () => await service.GetRecordsAsync("BOGUSTICKER", new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)));
        Assert.That(_requestCount, Is.EqualTo(1), "an empty time series was retried");
    }

    [Test]
    public void DataHub_GetNasdaqInstrumentsAsync_NoRows_FailsFastWithoutRetrying()
    {
        SetUpResponse("Symbol,Company Name\r\n", "text/csv");
        var service = new DataHubService(_mockHttpClientFactory.Object, _mockPolicyRegistry.Object);

        Assert.CatchAsync<FinanceNetNoDataException>(async () => await service.GetNasdaqInstrumentsAsync());
        Assert.That(_requestCount, Is.EqualTo(1), "an empty CSV was retried");
    }

    [Test]
    public void FinanceNetNoDataException_IsAFinanceNetException()
    {
        Assert.That(new FinanceNetNoDataException("no data"), Is.InstanceOf<FinanceNetException>());
    }
}

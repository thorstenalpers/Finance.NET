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
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Polly;
using Polly.Registry;

namespace Finance.Net.Tests.Services;

[TestFixture]
[Category("Unit")]
public class RetryableFailureTests
{
    private const int RetryCount = 3;
    private const string ConsentPage =
        "<html><body><form><h2>Before you continue to Yahoo</h2>" +
        "<button name='reject'>Reject all</button></form></body></html>";

    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;
    private int _requests;

    private void SetUpResponse(HttpStatusCode status, string body, string mediaType)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                _requests++;
                return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, mediaType) };
            });

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockHandler.Object));

        AsyncPolicy policy = Policy
            .Handle<Exception>(ex => ex is not FinanceNetNoDataException)
            .WaitAndRetryAsync(RetryCount, _ => TimeSpan.Zero);
        IAsyncPolicy asyncPolicy = policy;

        _mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
        _mockPolicyRegistry.Setup(registry => registry.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy)).Returns(policy);
        _mockPolicyRegistry.Setup(registry => registry.Get<IAsyncPolicy>(Constants.DefaultHttpRetryPolicy)).Returns(policy);
        _mockPolicyRegistry.Setup(registry => registry.TryGet(Constants.DefaultHttpRetryPolicy, out asyncPolicy)).Returns(true);
    }

    private YahooFinanceService CreateService()
    {
        var mockSession = new Mock<IYahooSessionManager>();
        return new YahooFinanceService(
            Mock.Of<ILogger<YahooFinanceService>>(),
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            mockSession.Object);
    }

    [SetUp]
    public void SetUp() => _requests = 0;

    [Test]
    public void GetSummaryAsync_ConsentPage_KeepsRetrying()
    {
        SetUpResponse(HttpStatusCode.OK, ConsentPage, "text/html");
        var service = CreateService();

        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetSummaryAsync("IBM"));

        Assert.That(exception, Is.Not.InstanceOf<FinanceNetNoDataException>());
        Assert.That(_requests, Is.EqualTo(RetryCount + 1),
            "the consent page was treated as a permanent no-data answer, so the cookie never got a second chance");
    }

    [Test]
    public void GetProfileAsync_ConsentPage_KeepsRetrying()
    {
        SetUpResponse(HttpStatusCode.OK, ConsentPage, "text/html");
        var service = CreateService();

        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetProfileAsync("IBM"));

        Assert.That(exception, Is.Not.InstanceOf<FinanceNetNoDataException>());
        Assert.That(_requests, Is.EqualTo(RetryCount + 1),
            "the consent page was treated as a permanent no-data answer, so the cookie never got a second chance");
    }

    [Test]
    public void GetInstrumentsAsync_ProviderOutage_IsNotReportedAsNoData()
    {
        SetUpResponse(HttpStatusCode.ServiceUnavailable, "", "text/html");
        var service = CreateService();

        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetInstrumentsAsync());

        Assert.That(exception, Is.Not.InstanceOf<FinanceNetNoDataException>(),
            "a 503 told the caller the instrument catalogue is permanently empty");
        Assert.That(exception.InnerException, Is.Not.Null, "the transport failure was dropped");
    }
}

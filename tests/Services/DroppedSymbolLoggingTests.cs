using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

/// <summary>
/// A batch quote request returns only the symbols Yahoo resolved. That leniency is what
/// lets a mixed batch survive one bad ticker, but the omission has to be observable -
/// otherwise a scheduled refresh cannot report which tickers went bad.
/// </summary>
[TestFixture]
[Category("Unit")]
public class DroppedSymbolLoggingTests
{
    private Mock<ILogger<YahooFinanceService>> _mockLogger;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;

    [SetUp]
    public void SetUp()
    {
        // The fixture resolves IBM only.
        var jsonContent = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "quote.json"));
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            });

        _mockLogger = new Mock<ILogger<YahooFinanceService>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockHandler.Object));

        var policy = Policy.Handle<Exception>().RetryAsync(0);
        _mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
        _mockPolicyRegistry.Setup(registry => registry.Get<AsyncPolicy>(Constants.DefaultHttpRetryPolicy)).Returns(policy);
    }

    private YahooFinanceService CreateService() => new(
        _mockLogger.Object,
        _mockHttpClientFactory.Object,
        _mockPolicyRegistry.Object,
        Mock.Of<IYahooSessionManager>());

    private void VerifyWarning(Func<string, bool> predicate, Times times, string because)
    {
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => predicate(v.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            times,
            because);
    }

    [Test]
    public async Task GetQuotesAsync_UnresolvedSymbol_LogsWarningNamingIt()
    {
        var result = await CreateService().GetQuotesAsync(["IBM", "BOGUSTICKER"]);

        Assert.That(result, Has.Exactly(1).Items, "the resolved symbol should still come back");
        VerifyWarning(message => message.Contains("BOGUSTICKER"), Times.Once(),
            "the dropped symbol was not named in any warning");
    }

    [Test]
    public async Task GetQuotesAsync_AllSymbolsResolved_LogsNothing()
    {
        await CreateService().GetQuotesAsync(["IBM"]);

        VerifyWarning(_ => true, Times.Never(), "a fully resolved batch should not warn");
    }

    [Test]
    public async Task GetQuotesAsync_SymbolCasingDiffers_IsNotReportedAsDropped()
    {
        await CreateService().GetQuotesAsync(["ibm"]);

        VerifyWarning(_ => true, Times.Never(), "symbol matching should ignore casing");
    }
}

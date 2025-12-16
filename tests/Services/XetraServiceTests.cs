using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Finance.Net.Exceptions;
using Finance.Net.Mappings;
using Finance.Net.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Polly;
using Polly.Registry;

namespace Finance.Net.Tests.Services;

[TestFixture]
[Category("Unit")]
public class XetraServiceTests
{
    private Mock<ILogger<XetraService>> _mockLogger;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<HttpMessageHandler> _mockHandler;
    private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;
    private IMapper _mapper;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<YahooQuoteAutomapperProfile>();
            cfg.AddProfile<XetraInstrumentAutomapperProfile>();
        }, new LoggerFactory());
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<XetraService>>();
        _mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
        var realPolicy = Policy.Handle<Exception>().RetryAsync(1);
        _mockPolicyRegistry
            .Setup(registry => registry.Get<IAsyncPolicy>(Constants.DefaultHttpRetryPolicy))
            .Returns(realPolicy);

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
    }

    [Test]
    public void Constructor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new XetraService(
            null,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mapper));
        Assert.Throws<ArgumentNullException>(() => new XetraService(
            _mockLogger.Object,
            null,
            _mockPolicyRegistry.Object,
            _mapper));
        Assert.Throws<ArgumentNullException>(() => new XetraService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            null,
            _mapper));
    }

    [Test]
    public async Task GetInstrumentsAsync_WithResponse_ReturnsResult()
    {
        // Arrange
        var csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "t7-xetr-allTradableInstruments.csv");
        var htmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "xetra-download-area.html");

        SetupHttpResponses(htmlFilePath, csvFilePath);

        var service = new XetraService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mapper);

        // Act
        var result = await service.GetInstrumentsAsync();

        // Assert

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.ISIN)));
        Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Mnemonic)));
    }

    [Test]
    public void GetInstrumentsAsync_NoHtml_Throws()
    {
        // Arrange
        var csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "t7-xetr-allTradableInstruments.csv");
        SetupHttpResponses(null, csvFilePath);

        var service = new XetraService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mapper);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetInstrumentsAsync());
    }

    [Test]
    public void GetInstrumentsAsync_NoCsv_Throws()
    {
        // Arrange
        var htmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "xetra-download-area.html");

        SetupHttpResponses(htmlFilePath, null);

        var service = new XetraService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mapper);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetInstrumentsAsync());
    }

    private void SetupHttpResponses(string htmlFilePath, string csvFilePath)
    {
        var htmlFilePathContent = string.IsNullOrEmpty(htmlFilePath) ? "" : File.ReadAllText(htmlFilePath);
        var csvFileContent = string.IsNullOrEmpty(csvFilePath) ? "" : File.ReadAllText(csvFilePath);

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(csvFileContent, Encoding.UTF8, "text/csv") });

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                                      ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri.StartsWith(Constants.XetraInstrumentsUrl, System.StringComparison.OrdinalIgnoreCase)),
                                      ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(htmlFilePathContent, Encoding.UTF8, "text/html") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
    }
}

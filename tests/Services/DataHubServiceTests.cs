using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
using Finance.Net.Services;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Polly;
using Polly.Registry;

namespace Finance.Net.Tests.Services;

[TestFixture]
[Category("Unit")]
public class DataHubServiceTests
{
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<HttpMessageHandler> _mockHandler;
    private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHandler = new Mock<HttpMessageHandler>();
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
        Assert.Throws<ArgumentNullException>(() => new DataHubService(
            null,
            _mockPolicyRegistry.Object));
        Assert.Throws<ArgumentNullException>(() => new DataHubService(
            _mockHttpClientFactory.Object,
            null));
    }

    [Test]
    public async Task GetNasdaqInstrumentsAsync_WithResponse_ReturnsResult()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "DataHub", "nasdaq-listed-symbols.csv");
        SetupHttpCsvFileResponse(filePath);
        var service = new DataHubService(
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object);

        // Act
        var result = await service.GetNasdaqInstrumentsAsync();

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Name)));
        Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Symbol)));
    }

    [Test]
    public void GetNasdaqInstrumentsAsync_ErrorResponse_Throws()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new DataHubService(
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object);

        // Act + Assert
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetNasdaqInstrumentsAsync());
        Assert.That(exception.Message, Does.Contain("No instruments found"));
        Assert.That(exception.InnerException.Message, Does.Contain("404"));
    }

    [Test]
    public void GetNasdaqInstrumentsAsync_EmptyResponse_Throws()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(" ") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new DataHubService(
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object);

        // Act + Assert
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetNasdaqInstrumentsAsync());
        Assert.That(exception.Message, Does.Contain("No instruments found"));
        Assert.That(exception.InnerException.Message, Does.Contain("was not found"));
    }

    [Test]
    public async Task GetSp500InstrumentsAsync_WithResponse_ReturnsResult()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "DataHub", "constituents-financials.csv");
        SetupHttpCsvFileResponse(filePath);
        var service = new DataHubService(
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object);

        // Act
        var result = await service.GetSp500InstrumentsAsync();

        // Assert

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Name)));
        Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Symbol)));
    }

    [Test]
    public void GetSp500InstrumentsAsync_ErrorResponse_Throws()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new DataHubService(
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object);

        // Act + Assert
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetSp500InstrumentsAsync());
        Assert.That(exception.Message, Does.Contain("No instruments found"));
        Assert.That(exception.InnerException.Message, Does.Contain("404"));
    }

    [Test]
    public void GetSp500InstrumentsAsync_EmptyResponse_Throws()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new DataHubService(
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object);

        // Act + Assert
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetSp500InstrumentsAsync());
        Assert.That(exception.Message, Does.Contain("No instruments found"));
        Assert.That(exception.InnerException.Message, Does.Contain("All fields empty"));
    }

    private void SetupHttpCsvFileResponse(string filePath)
    {
        var jsonContent = File.ReadAllText(filePath);
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonContent, Encoding.UTF8, "text/csv") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
    }
}

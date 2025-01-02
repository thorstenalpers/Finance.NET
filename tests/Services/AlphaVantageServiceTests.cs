using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Polly;
using Polly.Registry;

namespace Finance.Net.Tests.Services;

[TestFixture]
[Category("Unit")]
public class AlphaVantageServiceTests
{
    private Mock<ILogger<AlphaVantageService>> _mockLogger;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;
    private Mock<HttpMessageHandler> _mockHandler;
    private Mock<IOptions<FinanceNetConfiguration>> _mockOptions;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mockOptions = new Mock<IOptions<FinanceNetConfiguration>>();
        _mockOptions.Setup(x => x.Value).Returns(new FinanceNetConfiguration
        {
            HttpRetryCount = 1
        });
        _mockLogger = new Mock<ILogger<AlphaVantageService>>();
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
        Assert.Throws<ArgumentNullException>(() => new AlphaVantageService(
            null,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object));
        Assert.Throws<ArgumentNullException>(() => new AlphaVantageService(
            _mockLogger.Object,
            null,
            _mockOptions.Object,
            _mockPolicyRegistry.Object));
        Assert.Throws<ArgumentNullException>(() => new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            null,
            _mockPolicyRegistry.Object));
        Assert.Throws<ArgumentNullException>(() => new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            null));
    }

    [Test]
    public async Task GetOverviewAsync_WithData_ReturnsResult()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "companyOverview.json");
        var jsonContent = File.ReadAllText(filePath);
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonContent, Encoding.UTF8, "application/json") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        // Act
        var result = await service.GetOverviewAsync("IBM");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Symbol, Is.EqualTo("IBM"));
    }

    [Test]
    public void GetOverviewAsync_ApiThrottled_Throws()
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Buy higher API call volume!") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetOverviewAsync("IBM"));
    }

    [Test]
    public void GetOverviewAsync_NoData_Throws()
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("", Encoding.UTF8, "text/html"),
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        // Act
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetOverviewAsync("IBM"));
        Assert.That(exception.Message, Does.Contain("No overview "));
        Assert.That(exception.InnerException.Message, Does.Contain("All fields empty"));
    }

    [Test]
    public void GetOverviewAsync_EmptyData_Throws()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "empty.json");
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText(filePath), Encoding.UTF8, "text/html"),
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        // Act
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetOverviewAsync("IBM"));
        Assert.That(exception.Message, Does.Contain("No overview"));
        Assert.That(exception.InnerException.Message, Does.Contain("All fields empty"));
    }

    [Test]
    public async Task GetRecordsAsync_WithData_ReturnsResult()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "record.json");
        SetupHttpJsonFileResponse(filePath);

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 01, 01);

        // Act
        var result = await service.GetRecordsAsync("IBM", startDate);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(e => e.Open != null));
        Assert.That(result.All(e => e.Low != null));
        Assert.That(result.All(e => e.High != null));
        Assert.That(result.All(e => e.Close != null));
        Assert.That(result.All(e => e.Volume != null));
        Assert.That(result.All(e => e.AdjustedClose != null));
    }

    [Test]
    public void GetRecordsAsync_NoData_Throws()
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("", Encoding.UTF8, "text/html"),
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 01, 01);

        // Act
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetRecordsAsync("IBM", startDate));
        Assert.That(exception.Message, Does.Contain("No Record found "));
        Assert.That(exception.InnerException.Message, Does.Contain("data is invalid"));
    }

    [Test]
    public void GetRecordsAsync_EmptyData_Throws()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "record-empty.json");
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText(filePath), Encoding.UTF8, "text/html"),
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 01, 01);

        // Act
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetRecordsAsync("IBM", startDate));
        Assert.That(exception.Message, Does.Contain("No Record found "));
        Assert.That(exception.InnerException.Message, Does.Contain("All fields empty"));
    }


    [Test]
    public void GetRecordsAsync_StartdateAfterEnddate_Throws()
    {
        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 02, 01);
        var endDate = new DateTime(2024, 01, 01);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetRecordsAsync("IBM", startDate, endDate));
    }

    [Test]
    public void GetRecordsAsync_ApiThrottled_Throws()
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Buy higher API call volume!") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
        var startDate = new DateTime(2024, 01, 01);

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetRecordsAsync("IBM", startDate));
    }

    [Test]
    public async Task GetIntradayRecordsAsync_WithData_ReturnsResult()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "intradayRecord.json");
        SetupHttpJsonFileResponse(filePath);

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 01, 01);

        // Act
        var result = await service.GetIntradayRecordsAsync("IBM", startDate, null, EInterval.Interval_5Min);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(e => e.DateTime >= startDate));
    }

    [Test]
    public void GetIntradayRecordsAsync_NoData_Throws()
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("", Encoding.UTF8, "text/html"),
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 01, 01);

        // Act
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetIntradayRecordsAsync("IBM", startDate));
        Assert.That(exception.Message, Does.Contain("No intraday record"));
        Assert.That(exception.InnerException.Message, Does.Contain("no timeSeries"));
    }

    [Test]
    public void GetIntradayRecordsAsync_EmptyData_Throws()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "intradayRecord-empty.json");
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText(filePath), Encoding.UTF8, "text/html"),
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 01, 01);

        // Act
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetIntradayRecordsAsync("IBM", startDate));
        Assert.That(exception.Message, Does.Contain("No intraday record"));
        Assert.That(exception.InnerException.Message, Does.Contain("All fields empty"));
    }

    [Test]
    public void GetIntradayRecordsAsync_ApiThrottled_Throws()
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Buy higher API call volume!") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
        var startDate = new DateTime(2024, 01, 01);

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetIntradayRecordsAsync("IBM", startDate));
    }

    [Test]
    public void GetIntradayRecordsAsync_StartdateAfterEnddate_Throws()
    {
        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 02, 01);
        var endDate = new DateTime(2024, 01, 01);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetIntradayRecordsAsync("IBM", startDate, endDate));
    }

    [Test]
    public void GetIntradayRecordsAsync_IvalidInterval_Throws()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "intradayRecord.json");
        SetupHttpJsonFileResponse(filePath);

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 01, 01);
        var endDate = new DateTime(2024, 02, 01);
        var interval = (EInterval)100;

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetIntradayRecordsAsync("IBM", startDate, endDate, interval));
    }

    [Test]
    public async Task GetForexRecordsAsync_WithData_ReturnsResult()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "forex.json");
        SetupHttpJsonFileResponse(filePath);

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 11, 01);

        // Act
        var result = await service.GetForexRecordsAsync("EUR", "USD", startDate);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(e => e.Open != null));
        Assert.That(result.All(e => e.Date != null));
        Assert.That(result.All(e => e.Low != null));
        Assert.That(result.All(e => e.High != null));
        Assert.That(result.All(e => e.Close != null));
    }

    [Test]
    public void GetForexRecordsAsync_NoData_Throws()
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("", Encoding.UTF8, "text/html"),
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 11, 01);

        // Act
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetForexRecordsAsync("EUR", "USD", startDate));
        Assert.That(exception.Message, Does.Contain("No forex record found"));
        Assert.That(exception.InnerException.Message, Does.Contain("data is invalid"));
    }

    [Test]
    public void GetForexRecordsAsync_EmptyData_Throws()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "forex-empty.json");
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText(filePath), Encoding.UTF8, "text/html"),
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 11, 01);

        // Act
        var exception = Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetForexRecordsAsync("EUR", "USD", startDate));
        Assert.That(exception.Message, Does.Contain("No forex record found"));
        Assert.That(exception.InnerException.Message, Does.Contain("All fields empty"));
    }

    [Test]
    public void GetForexRecordsAsync_ApiThrottled_Throws()
    {
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Buy higher API call volume!") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
        var startDate = new DateTime(2024, 11, 01);

        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetForexRecordsAsync("EUR", "USD", startDate));
    }


    [Test]
    public void GetForexRecordsAsync_StartdateAfterEnddate_Throws()
    {
        var service = new AlphaVantageService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockPolicyRegistry.Object);

        var startDate = new DateTime(2024, 02, 01);
        var endDate = new DateTime(2024, 01, 01);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetForexRecordsAsync("EUR", "USD", startDate, endDate));
    }

    private void SetupHttpJsonFileResponse(string filePath)
    {
        var jsonContent = File.ReadAllText(filePath);
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(jsonContent, Encoding.UTF8, "application/json") });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
    }
}

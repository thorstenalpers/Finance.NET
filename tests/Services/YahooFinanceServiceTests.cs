using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
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
public class YahooFinanceServiceTests
{
    private Mock<ILogger<YahooFinanceService>> _mockLogger;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IYahooSessionManager> _mockYahooSession;
    private Mock<HttpMessageHandler> _mockHandler;
    private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mockLogger = new Mock<ILogger<YahooFinanceService>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHandler = new Mock<HttpMessageHandler>();
        _mockYahooSession = new Mock<IYahooSessionManager>();
        _mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();

        var realPolicy = Policy.Handle<Exception>().RetryAsync(1);
        _mockPolicyRegistry
            .Setup(registry => registry.Get<IAsyncPolicy>(Constants.DefaultHttpRetryPolicy))
            .Returns(realPolicy);

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("", Encoding.UTF8, "text/html"),
                Headers =
                {
                    { "Set-Cookie", "\"A3=d=AQABBIPiUmcCEKLS0S2dxFEvSY2wq0BTJc4FEgEBAQE0VGdcZ-AMyiMA_eMAAA&S=AQAAAueeOka9YBgG-7Z2662G2t0; Expires=Mo, 10 Dec 2040 17:39:47 GMT; Max-Age=99931557600; Domain=.yahoo.com; Path=/; SameSite=None; Secure; HttpOnly\"" } // Add custom headers here
                            }
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
    }

    [Test]
    public void Constructor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new YahooFinanceService(
            null,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object));
        Assert.Throws<ArgumentNullException>(() => new YahooFinanceService(
            _mockLogger.Object,
            null,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object));
        Assert.Throws<ArgumentNullException>(() => new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            null,
            _mockYahooSession.Object));
        Assert.Throws<ArgumentNullException>(() => new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            null));
    }

    [Test]
    public void Create_Static_ReturnsObject()
    {
        // Act
        var service1 = YahooFinanceService.Create();
        var service2 = YahooFinanceService.Create(new FinanceNetConfiguration());

        // Assert
        Assert.That(service1, Is.Not.Null);
        Assert.That(service2, Is.Not.Null);
    }

    [Test]
    public async Task GetQuoteAsync_WithResponse_ReturnsResult()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "quote.json");
        SetupHttpJsonFileResponse(filePath);

        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        var result = await service.GetQuoteAsync("IBM");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Symbol, Is.EqualTo("IBM"));
        Assert.That(!string.IsNullOrWhiteSpace(result.ShortName));
        Assert.That(result.MarketCap > 0);
    }

    [Test]
    public void GetQuoteAsync_NoResponse_Throws()
    {
        // Arrange
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetQuoteAsync("IBM"));
    }

    [Test]
    public void GetQuoteAsync_EmptyResponse_Throws()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "quote_empty_response.json");
        SetupHttpJsonFileResponse(filePath);

        // Arrange
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetQuoteAsync("IBM"));
    }

    [Test]
    public void GetQuoteAsync_ErrorResponse_Throws()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "quote_error_response.json");
        SetupHttpJsonFileResponse(filePath);

        // Arrange
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetQuoteAsync("IBM"));
    }

    [Test]
    public void GetQuoteAsync_InvalidResponse_Throws()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "quote_invalid_response.json");
        SetupHttpJsonFileResponse(filePath);

        // Arrange
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetQuoteAsync("SAP"));
    }

    [Test]
    public async Task GetQuotesAsync_WithResponse_ReturnsResult()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "quote.json");
        SetupHttpJsonFileResponse(filePath);

        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        var symbols = new List<string> { "IBM" };

        // Act
        var result = await service.GetQuotesAsync(symbols);

        // Assert
        Assert.That(result, Is.Not.Empty);
        var first = result.FirstOrDefault();
        Assert.That(first.Symbol, Is.EqualTo(symbols.FirstOrDefault()));
        Assert.That(!string.IsNullOrWhiteSpace(first.ShortName));
        Assert.That(first.MarketCap > 0);
    }

    [Test]
    public async Task GetProfileAsync_WithResponse_ReturnsResult()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "profile.html");
        SetupHttpHtmlFileResponse(filePath);

        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        var result = await service.GetProfileAsync("IBM");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(!string.IsNullOrWhiteSpace(result.Phone));
        Assert.That(!string.IsNullOrWhiteSpace(result.Description));
    }
    [Test]
    public void GetProfileAsync_NoResponse_Throws()
    {
        // Arrange
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetProfileAsync("IBM"));
    }

    [Test]
    public async Task GetRecordsAsync_WithResponse_ReturnsResult()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "records.html");
        SetupHttpHtmlFileResponse(filePath);
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        DateTime startDate = default;

        // Act
        var result = await service.GetRecordsAsync("IBM", startDate);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.All(e => e.Open != null));
        Assert.That(result.All(e => e.Close != null));
        Assert.That(result.All(e => e.AdjustedClose != null));
        Assert.That(result.All(e => e.Low != null));
        Assert.That(result.All(e => e.High != null));
    }
    [Test]
    public void GetRecordsAsync_NoResponse_Throws()
    {
        // Arrange
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        DateTime startDate = default;

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetRecordsAsync("IBM", startDate));
    }

    [Test]
    public async Task GetFinancialReportsAsync_WithResponseEuConsent_ReturnsResult()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "financial_eu.html");
        SetupHttpHtmlFileResponse(filePath);

        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        var result = await service.GetFinancialReportsAsync("IBM");

        // Assert
        Assert.That(result, Is.Not.Empty);
        var first = result.First();
        Assert.That(!string.IsNullOrWhiteSpace(first.Key));
        Assert.That(first.Value.TotalRevenue != null);
        Assert.That(first.Value.BasicAverageShares != null);
        Assert.That(first.Value.EBIT != null);
    }

    [Test]
    public async Task GetFinancialReportsAsync_WithResponseUs_ReturnsResult()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "financial_us.html");
        SetupHttpHtmlFileResponse(filePath);

        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        var result = await service.GetFinancialReportsAsync("IBM");

        // Assert
        Assert.That(result, Is.Not.Empty);
        var first = result.First();
        Assert.That(!string.IsNullOrWhiteSpace(first.Key));
        Assert.That(first.Value.TotalRevenue != null);
        Assert.That(first.Value.BasicAverageShares != null);
        Assert.That(first.Value.EBIT != null);
    }
    [Test]
    public void GetFinancialReportsAsync_NoResponse_Throws()
    {
        // Arrange
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        // Assert
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetFinancialReportsAsync("IBM"));
    }

    [Test]
    public async Task GetSummaryAsync_WithResponse_ReturnsResult()
    {
        // Arrange		
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "summary.html");
        SetupHttpHtmlFileResponse(filePath);

        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        var result = await service.GetSummaryAsync("IBM");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.PreviousClose != null);
        Assert.That(result.Ask != null);
        Assert.That(result.Bid != null);
        Assert.That(result.MarketCap_Intraday != null);
    }
    [Test]
    public void GetSummaryAsync_NoResponse_Throws()
    {
        // Arrange		
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetSummaryAsync("IBM"));
    }

    [Test]
    public void GetSymbolsAsync_NoResponse_Throws()
    {
        // Arrange		
        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetSymbolsAsync());
    }

    [TestCase(EInstrumentType.Index, 41)]
    [TestCase(EInstrumentType.Stock, 25)]
    [TestCase(EInstrumentType.ETF, 25)]
    [TestCase(EInstrumentType.Forex, 23)]
    [TestCase(EInstrumentType.Crypto, 25)]
    [TestCase(null, 139)]
    public async Task GetSymbolsAsync_WithResponse_ReturnsResult(EInstrumentType? type, int expectedCnt)
    {
        // Arrange
        var filePathIndex = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "symbols_indices.html");
        var filePathStocks = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "symbols_stocks.html");
        var filePathEtfs = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "symbols_etfs.html");
        var filePathForex = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "symbols_forex.html");
        var filePathCryptos = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Yahoo", "symbols_cryptos.html");

        if (type == null)
        {
            SetupHttpHtmlFileResponses([filePathStocks, filePathEtfs, filePathCryptos, filePathIndex, filePathForex]);
        }
        else
        {
            var filePath = type switch
            {
                EInstrumentType.Index => filePathIndex,
                EInstrumentType.Stock => filePathStocks,
                EInstrumentType.ETF => filePathEtfs,
                EInstrumentType.Forex => filePathForex,
                EInstrumentType.Crypto => filePathCryptos,
                _ => throw new NotImplementedException(),
            };
            SetupHttpHtmlFileResponse(filePath);
        }

        var service = new YahooFinanceService(
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            _mockPolicyRegistry.Object,
            _mockYahooSession.Object);

        // Act
        var result = await service.GetSymbolsAsync(type);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(expectedCnt));
    }

    private void SetupHttpHtmlFileResponse(string filePath)
    {
        var jsonContent = File.ReadAllText(filePath);
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "text/html"),
                Headers =
                {
                    { "Set-Cookie", "\"A3=d=AQABBIPiUmcCEKLS0S2dxFEvSY2wq0BTJc4FEgEBAQE0VGdcZ-AMyiMA_eMAAA&S=AQAAAueeOka9YBgG-7Z2662G2t0; Expires=Mo, 10 Dec 2040 17:39:47 GMT; Max-Age=99931557600; Domain=.yahoo.com; Path=/; SameSite=None; Secure; HttpOnly\"" } // Add custom headers here
                            }
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
    }

    private void SetupHttpHtmlFileResponses(List<string> filePaths)
    {
        var setupSequence = _mockHandler
        .Protected()
        .SetupSequence<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );

        foreach (var filePath in filePaths)
        {
            setupSequence = setupSequence.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText(filePath), Encoding.UTF8, "text/html"),
                Headers =
            {
                { "Set-Cookie", "\"A3=d=AQABBIPiUmcCEKLS0S2dxFEvSY2wq0BTJc4FEgEBAQE0VGdcZ-AMyiMA_eMAAA&S=AQAAAueeOka9YBgG-7Z2662G2t0; Expires=Mo, 10 Dec 2040 17:39:47 GMT; Max-Age=99931557600; Domain=.yahoo.com; Path=/; SameSite=None; Secure; HttpOnly\"" }
            }
            });
        }
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
    }

    private void SetupHttpJsonFileResponse(string filePath)
    {
        var jsonContent = File.ReadAllText(filePath);
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "text/html"),
                Headers =
                {
                    { "Set-Cookie", "\"A3=d=AQABBIPiUmcCEKLS0S2dxFEvSY2wq0BTJc4FEgEBAQE0VGdcZ-AMyiMA_eMAAA&S=AQAAAueeOka9YBgG-7Z2662G2t0; Expires=Mo, 10 Dec 2040 17:39:47 GMT; Max-Age=99931557600; Domain=.yahoo.com; Path=/; SameSite=None; Secure; HttpOnly\"" } // Add custom headers here
                            }
            });
        _mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
    }
}

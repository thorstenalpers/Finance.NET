using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
using Finance.Net.Models.AlphaVantage;
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
		public void Create_Static_ReturnsObject()
		{
				// Arrange
				FinanceNetConfiguration cfg = null;

				// Act
				var service = AlphaVantageService.Create(cfg);

				// Assert
				Assert.That(service, Is.Not.Null);
		}

		[Test]
		public async Task GetCompanyOverviewAsync_WithData_ReturnsResult()
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
				var result = await service.GetCompanyOverviewAsync("IBM");

				// Assert
				Assert.That(result, Is.Not.Null);
				Assert.That(result.Symbol, Is.EqualTo("IBM"));
		}

		[Test]
		public void GetCompanyOverviewAsync_ApiThrottled_Throws()
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
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetCompanyOverviewAsync("IBM"));
		}

		[Test]
		public void GetCompanyOverviewAsync_NoData_Throws()
		{
				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				// Act
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetCompanyOverviewAsync("IBM"));
		}

		[Test]
		public async Task GetHistoryRecordsAsync_WithData_ReturnsResult()
		{
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "record.json");
				SetupHttpJsonFileResponse(filePath);

				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				var startDate = new DateTime(2024, 01, 01);
				DateTime? endDate = null;

				// Act
				var result = await service.GetHistoryRecordsAsync(
						"IBM",
						startDate,
						endDate);

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
		public void GetHistoryRecordsAsync_NoData_Throws()
		{
				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				var startDate = new DateTime(2024, 01, 01);
				DateTime? endDate = null;

				// Act
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetHistoryRecordsAsync("IBM", startDate, endDate));
		}

		[Test]
		public void GetHistoryRecordsAsync_ApiThrottled_Throws()
		{
				_mockHandler
						.Protected()
						.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Buy higher API call volume!") });
				_mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
				var startDate = new DateTime(2024, 01, 01);
				DateTime? endDate = null;

				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				// Act
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetHistoryRecordsAsync("IBM", startDate, endDate));
		}

		[Test]
		public async Task GetHistoryIntradayRecordsAsync_WithData_ReturnsResult()
		{
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "intradayRecord.json");
				SetupHttpJsonFileResponse(filePath);

				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				var startDate = new DateTime(2024, 01, 01);
				DateTime? endDate = null;

				// Act
				var result = await service.GetHistoryIntradayRecordsAsync("IBM", startDate, endDate, EInterval.Interval_5Min);

				// Assert
				Assert.That(result, Is.Not.Empty);
				Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.DateOnly)));
				Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.TimeOnly)));
		}

		[Test]
		public void GetHistoryIntradayRecordsAsync_NoData_Throws()
		{
				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				var startDate = new DateTime(2024, 01, 01);
				DateTime? endDate = null;

				// Act
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetHistoryIntradayRecordsAsync("IBM", startDate, endDate, EInterval.Interval_5Min));
		}

		[Test]
		public void GetHistoryIntradayRecordsAsync_ApiThrottled_Throws()
		{
				_mockHandler
						.Protected()
						.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Buy higher API call volume!") });
				_mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
				var startDate = new DateTime(2024, 01, 01);
				DateTime? endDate = null;

				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				// Act
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetHistoryIntradayRecordsAsync("IBM", startDate, endDate, EInterval.Interval_5Min));
		}

		[Test]
		public async Task GetHistoryForexRecordsAsync_WithData_ReturnsResult()
		{
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "AlphaVantage", "forex.json");
				SetupHttpJsonFileResponse(filePath);

				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				var startDate = new DateTime(2024, 11, 01);
				DateTime? endDate = null;

				// Act
				var result = await service.GetHistoryForexRecordsAsync(
						"EUR",
						"USD",
						startDate,
						endDate);

				// Assert
				Assert.That(result, Is.Not.Empty);
				Assert.That(result.All(e => e.Open != null));
				Assert.That(result.All(e => e.Date != null));
				Assert.That(result.All(e => e.Low != null));
				Assert.That(result.All(e => e.High != null));
				Assert.That(result.All(e => e.Close != null));
		}

		[Test]
		public void GetHistoryForexRecordsAsync_NoData_Throws()
		{
				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				var startDate = new DateTime(2024, 11, 01);
				DateTime? endDate = null;

				// Act
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetHistoryForexRecordsAsync(
						"EUR",
						"USD",
						startDate,
						endDate));
		}

		[Test]
		public void GetHistoryForexRecordsAsync_ApiThrottled_Throws()
		{
				_mockHandler
						.Protected()
						.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Buy higher API call volume!") });
				_mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
				var startDate = new DateTime(2024, 11, 01);
				DateTime? endDate = null;

				var service = new AlphaVantageService(
						_mockLogger.Object,
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				// Act
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetHistoryForexRecordsAsync(
						"EUR",
						"USD",
						startDate,
						endDate));
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

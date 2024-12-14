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
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Polly;
using Polly.Registry;

namespace Finance.Net.Tests.Services;

[TestFixture]
[Category("Unit")]
public class DatahubIoServiceTests
{
		private Mock<IHttpClientFactory> _mockHttpClientFactory;
		private Mock<IOptions<FinanceNetConfiguration>> _mockOptions;
		private Mock<HttpMessageHandler> _mockHandler;
		private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
				_mockOptions = new Mock<IOptions<FinanceNetConfiguration>>();
				_mockOptions.Setup(x => x.Value).Returns(new FinanceNetConfiguration());
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
				Assert.Throws<ArgumentNullException>(() => new DatahubIoService(
						null,
						_mockOptions.Object,
						_mockPolicyRegistry.Object));
				Assert.Throws<ArgumentNullException>(() => new DatahubIoService(
						_mockHttpClientFactory.Object,
						null,
						_mockPolicyRegistry.Object));
				Assert.Throws<ArgumentNullException>(() => new DatahubIoService(
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
				var service = DatahubIoService.Create(cfg);

				// Assert
				Assert.That(service, Is.Not.Null);
		}

		[Test]
		public async Task GetNasdaqInstrumentsAsync_WithResponse_ReturnsResult()
		{
				// Arrange
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "DatahubIo", "nasdaq-listed-symbols.csv");
				SetupHttpCsvFileResponse(filePath);
				var service = new DatahubIoService(
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				// Act
				var result = await service.GetNasdaqInstrumentsAsync();

				// Assert
				Assert.That(result, Is.Not.Empty);
				Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.CompanyName)));
				Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Symbol)));
		}

		[Test]
		public void GetNasdaqInstrumentsAsync_NoResponse_Throws()
		{
				// Arrange
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "DatahubIo", "nasdaq-listed-symbols.csv");
				SetupHttpCsvFileResponse(filePath);
				_mockHandler
						.Protected()
						.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") });
				_mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

				var service = new DatahubIoService(
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				// Act + Assert
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetNasdaqInstrumentsAsync());
		}

		[Test]
		public async Task GetSAndP500InstrumentsAsync_WithResponse_ReturnsResult()
		{
				// Arrange
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "DatahubIo", "constituents-financials.csv");
				SetupHttpCsvFileResponse(filePath);
				var service = new DatahubIoService(
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				// Act
				var result = await service.GetSP500InstrumentsAsync();

				// Assert

				Assert.That(result, Is.Not.Empty);
				Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Name)));
				Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Symbol)));
		}

		[Test]
		public void GetSAndP500InstrumentsAsync_NoResponse_Throws()
		{
				// Arrange
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "DatahubIo", "constituents-financials.csv");
				SetupHttpCsvFileResponse(filePath);
				_mockHandler
						.Protected()
						.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") });
				_mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
				_mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

				var service = new DatahubIoService(
						_mockHttpClientFactory.Object,
						_mockOptions.Object,
						_mockPolicyRegistry.Object);

				// Act + Assert
				Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetSP500InstrumentsAsync());
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

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
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
public class XetraServiceTests
{
	private Mock<ILogger<IXetraService>> _mockLogger;
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
		_mockLogger = new Mock<ILogger<IXetraService>>();
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
	public void Create_Static_ReturnsObject()
	{
		// Arrange
		FinanceNetConfiguration cfg = null;

		// Act
		var service = XetraService.Create(cfg);

		// Assert
		Assert.That(service, Is.Not.Null);
	}

	[Test]
	public async Task GetTradableInstruments_WithResponse_ReturnsResult()
	{
		// Arrange
		var csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "t7-xetr-allTradableInstruments.csv");
		var htmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "xetra-download-area.html");

		SetupHttpResponses(htmlFilePath, csvFilePath);

		var service = new XetraService(
				_mockLogger.Object,
				_mockHttpClientFactory.Object,
				_mockOptions.Object,
				_mockPolicyRegistry.Object);

		// Act
		var result = await service.GetInstruments();

		// Assert

		Assert.That(result, Is.Not.Empty);
		Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.ISIN)));
		Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Mnemonic)));
	}

	[Test]
	public void GetTradableInstruments_NoHtml_Throws()
	{
		// Arrange
		var csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "t7-xetr-allTradableInstruments.csv");

		SetupHttpResponses(null, csvFilePath);

		var service = new XetraService(
				_mockLogger.Object,
				_mockHttpClientFactory.Object,
				_mockOptions.Object,
				_mockPolicyRegistry.Object);

		// Act
		Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetInstruments());
	}

	[Test]
	public void GetTradableInstruments_NoCsv_Throws()
	{
		// Arrange
		var htmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "xetra-download-area.html");

		SetupHttpResponses(htmlFilePath, null);

		var service = new XetraService(
				_mockLogger.Object,
				_mockHttpClientFactory.Object,
				_mockOptions.Object,
				_mockPolicyRegistry.Object);

		// Act
		Assert.ThrowsAsync<FinanceNetException>(async () => await service.GetInstruments());
	}

	private void SetupHttpResponses(string htmlFilePath, string csvFilePath)
	{
		var htmlFilePathContent = string.IsNullOrEmpty(htmlFilePath) ? "" : File.ReadAllText(htmlFilePath);
		var csvFileContent = string.IsNullOrEmpty(csvFilePath) ? "" : File.ReadAllText(csvFilePath);

		_mockHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync",
						ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase)),
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

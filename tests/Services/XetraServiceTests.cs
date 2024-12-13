using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Interfaces;
using Finance.Net.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Finance.Net.Tests.Services;

[TestFixture]
[Category("UnitTests")]
public class XetraServiceTests
{
	private Mock<ILogger<IXetraService>> _mockLogger;
	private Mock<IHttpClientFactory> _mockHttpClientFactory;
	private Mock<IOptions<FinanceNetConfiguration>> _mockOptions;
	private Mock<HttpMessageHandler> _mockHandler;

	[SetUp]
	public void SetUp()
	{
		_mockOptions = new Mock<IOptions<FinanceNetConfiguration>>();
		_mockOptions.Setup(x => x.Value).Returns(new FinanceNetConfiguration { });
		_mockHttpClientFactory = new Mock<IHttpClientFactory>();
		_mockHandler = new Mock<HttpMessageHandler>();
		_mockLogger = new Mock<ILogger<IXetraService>>();

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
	public async Task GetTradableInstruments_WithValidEntries_ReturnsResult()
	{
		// Arrange
		var csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "t7-xetr-allTradableInstruments.csv");
		var htmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "Xetra", "xetra-download-area.html");

		SetupHttpResponses(htmlFilePath, csvFilePath);

		var service = new XetraService(
			_mockLogger.Object,
			_mockHttpClientFactory.Object,
			_mockOptions.Object);

		// Act
		var result = await service.GetInstruments();

		// Assert

		Assert.That(result, Is.Not.Empty);
		Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.ISIN)));
		Assert.That(result.All(e => !string.IsNullOrWhiteSpace(e.Mnemonic)));
	}

	private void SetupHttpResponses(string htmlFilePath, string csvFilePath)
	{
		var htmlFilePathContent = File.ReadAllText(htmlFilePath);
		var csvFileContent = File.ReadAllText(csvFilePath);

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

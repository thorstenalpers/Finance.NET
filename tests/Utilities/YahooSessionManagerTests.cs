using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Finance.Net.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Polly;
using Polly.Registry;

namespace Finance.Net.Tests.Utilities;

[TestFixture]
[Category("Unit")]
public class YahooSessionManagerTests
{
	private Mock<IHttpClientFactory> _mockHttpClientFactory;
	private Mock<ILogger<IYahooSessionManager>> _mockLogger;
	private Mock<IYahooSessionState> _mockYahooSessionState;
	private Mock<HttpMessageHandler> _mockHandler;
	private Mock<IReadOnlyPolicyRegistry<string>> _mockPolicyRegistry;

	[SetUp]
	public void SetUp()
	{
		_mockHttpClientFactory = new Mock<IHttpClientFactory>();
		_mockLogger = new Mock<ILogger<IYahooSessionManager>>();
		_mockYahooSessionState = new Mock<IYahooSessionState>();
		_mockHttpClientFactory = new Mock<IHttpClientFactory>();
		_mockHandler = new Mock<HttpMessageHandler>();
		var realPolicy = Policy.Handle<Exception>().RetryAsync(1);
		_mockPolicyRegistry = new Mock<IReadOnlyPolicyRegistry<string>>();
		_mockPolicyRegistry
								.Setup(registry => registry.Get<IAsyncPolicy>(Constants.DefaultHttpRetryPolicy))
				.Returns(realPolicy);
		_mockHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("Crumb"),
					Headers = { { "Set-Cookie", "cookieName=cookieValue; path=/; expires=Wed, 13 Jan 2024 00:00:00 GMT" } }
				});

		_mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));
	}

	[Test]
	public void GetApiCrumb_Initialized_ReturnsSame()
	{
		// Arrange
		const string expected = "abc123";
		_mockYahooSessionState.Setup(s => s.GetCrumb()).Returns(expected);

		var manager = new YahooSessionManager(
				_mockHttpClientFactory.Object,
				_mockLogger.Object,
				_mockYahooSessionState.Object,
				_mockPolicyRegistry.Object);

		// Act
		var result = manager.GetApiCrumb();

		// Assert
		Assert.That(expected, Is.EqualTo(result));
		_mockYahooSessionState.Verify(s => s.GetCrumb(), Times.Once);
	}

	[Test]
	public void GetUserAgent_Initialized_ReturnsSame()
	{
		// Arrange
		const string expected = "userAgent";
		_mockYahooSessionState.Setup(s => s.GetUserAgent()).Returns(expected);

		var manager = new YahooSessionManager(
				_mockHttpClientFactory.Object,
				_mockLogger.Object,
				_mockYahooSessionState.Object,
				_mockPolicyRegistry.Object);

		// Act
		var result = manager.GetUserAgent();

		// Assert
		Assert.That(expected, Is.EqualTo(result));
		_mockYahooSessionState.Verify(s => s.GetUserAgent(), Times.Once);
	}

	[Test]
	public void GetCookies_Initialized_ReturnsValidCookies()
	{
		// Arrange
		var cookieContainer = new CookieContainer();
		var cookie = new Cookie("cookieName", "Value", "/", ".yahoo.com");
		cookieContainer.Add(cookie);
		_mockYahooSessionState.Setup(s => s.IsValid()).Returns(true);
		_mockYahooSessionState.Setup(s => s.GetCookieContainer()).Returns(cookieContainer);

		var manager = new YahooSessionManager(
				_mockHttpClientFactory.Object,
				_mockLogger.Object,
				_mockYahooSessionState.Object,
				_mockPolicyRegistry.Object);

		// Act
		var result = manager.GetCookies();

		// Assert
		Assert.That(cookieContainer.Count, Is.EqualTo(result.Count()));
		Assert.That(cookieContainer.GetAllCookies().FirstOrDefault().Name, Is.EqualTo(result.FirstOrDefault().Name));
	}

	[Test]
	public async Task RefreshSessionAsync_AlreadyRefreshed_Aborts()
	{
		// Arrange
		const bool expected = true;
		_mockYahooSessionState.Setup(s => s.IsValid()).Returns(expected);
		_mockYahooSessionState.Setup(s => s.GetCookieContainer()).Returns(new CookieContainer());

		var manager = new YahooSessionManager(
				_mockHttpClientFactory.Object,
				_mockLogger.Object,
				_mockYahooSessionState.Object,
				_mockPolicyRegistry.Object);

		// Act
		await manager.RefreshSessionAsync();

		// Assert
		_mockYahooSessionState.Verify(s => s.IsValid(), Times.Once);
	}

	[Test]
	public async Task RefreshSessionAsync_NotRefreshed_Refreshes()
	{
		// Arrange
		var cookieContainer = new CookieContainer();
		cookieContainer.Add(new Cookie("cookieName1", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		cookieContainer.Add(new Cookie("cookieName2", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		cookieContainer.Add(new Cookie("cookieName3", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		cookieContainer.Add(new Cookie("cookieName4", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		_mockYahooSessionState.Setup(s => s.GetCookieContainer()).Returns(cookieContainer);
		_mockYahooSessionState.SetupSequence(s => s.IsValid())
				.Returns(false)
				.Returns(true);
		var manager = new YahooSessionManager(
				_mockHttpClientFactory.Object,
				_mockLogger.Object,
				_mockYahooSessionState.Object,
				_mockPolicyRegistry.Object);

		// Act
		await manager.RefreshSessionAsync();

		// Assert
		_mockYahooSessionState.Verify(s => s.IsValid(), Times.AtLeast(2));
	}

	[Test]
	public async Task RefreshSessionAsync_NotRefreshed_RefreshesWithEuConsent()
	{
		// Arrange
		var cookieContainer = new CookieContainer();
		cookieContainer.Add(new Cookie("cookieName1", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		cookieContainer.Add(new Cookie("cookieName2", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		cookieContainer.Add(new Cookie("cookieName3", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		cookieContainer.Add(new Cookie("cookieName4", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		_mockYahooSessionState.Setup(s => s.GetCookieContainer()).Returns(cookieContainer);
		_mockYahooSessionState.SetupSequence(s => s.IsValid())
				.Returns(false)
				.Returns(true);
		_mockHandler
				.Protected()
				.SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("Auth"),
					Headers = { { "Set-Cookie", "cookieName=cookieValue; path=/; expires=Wed, 13 Jan 2024 00:00:00 GMT" } }
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("Crumb"),
					Headers = { { "Set-Cookie", "cookieName=cookieValue; path=/; expires=Wed, 13 Jan 2024 00:00:00 GMT" } }
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("<div class=\"actions couple\">\r\n    <input type=\"hidden\" name=\"csrfToken\" value=\"W95-xeA\">\r\n    <input type=\"hidden\" name=\"sessionId\" value=\"3_cc-session_41bb06e7-1410-408d-9312-8fdb41fc92a2\">\r\n    <input type=\"hidden\" name=\"originalDoneUrl\" value=\"https://finance.yahoo.com/quote/SAP.DE/profile/?guccounter=2\">\r\n    <input type=\"hidden\" name=\"namespace\" value=\"yahoo\">\r\n    <button type=\"submit\" class=\"btn secondary accept-all \" name=\"agree\" value=\"agree\">Alle akzeptieren</button>\r\n        <button type=\"submit\" class=\"btn secondary reject-all\" name=\"reject\" value=\"reject\">Alle ablehnen</button>\r\n    <a href=\"/v2/partners?sessionId=3_cc-session_41bb06e7-1410-408d-9312-8fdb41fc92a2\" class=\"btn secondary mng-btn manage-settings\" role=\"button\">Datenschutzeinstellungen verwalten</a>\r\n                    </div>"),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				});
		_mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

		var manager = new YahooSessionManager(
				_mockHttpClientFactory.Object,
				_mockLogger.Object,
				_mockYahooSessionState.Object,
				_mockPolicyRegistry.Object);

		// Act
		await manager.RefreshSessionAsync();

		// Assert
		_mockYahooSessionState.Verify(s => s.IsValid(), Times.AtLeast(2));

		_mockLogger.Verify(
				logger => logger.Log(
						LogLevel.Information,
						It.IsAny<EventId>(),
						It.Is<It.IsAnyType>((state, _) =>
						state.ToString() == "UI Session established successfully"),
						It.IsAny<Exception>(),
						It.IsAny<Func<It.IsAnyType, Exception, string>>()
				),
				Times.Once);
	}

	[Test]
	public void RefreshSessionAsync_SessionInvalid_Throws()
	{
		// Arrange
		var cookieContainer = new CookieContainer();
		cookieContainer.Add(new Cookie("cookieName1", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		cookieContainer.Add(new Cookie("cookieName2", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		cookieContainer.Add(new Cookie("cookieName3", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		cookieContainer.Add(new Cookie("cookieName4", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		});
		_mockYahooSessionState.Setup(s => s.GetCookieContainer()).Returns(cookieContainer);
		_mockYahooSessionState.Setup(s => s.IsValid())
				.Returns(false);
		_mockHandler
				.Protected()
				.SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("Auth"),
					Headers = { { "Set-Cookie", "cookieName=cookieValue; path=/; expires=Wed, 13 Jan 2024 00:00:00 GMT" } }
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("Crumb"),
					Headers = { { "Set-Cookie", "cookieName=cookieValue; path=/; expires=Wed, 13 Jan 2024 00:00:00 GMT" } }
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("<div class=\"actions couple\">\r\n    <input type=\"hidden\" name=\"csrfToken\" value=\"W95-xeA\">\r\n    <input type=\"hidden\" name=\"sessionId\" value=\"3_cc-session_41bb06e7-1410-408d-9312-8fdb41fc92a2\">\r\n    <input type=\"hidden\" name=\"originalDoneUrl\" value=\"https://finance.yahoo.com/quote/SAP.DE/profile/?guccounter=2\">\r\n    <input type=\"hidden\" name=\"namespace\" value=\"yahoo\">\r\n    <button type=\"submit\" class=\"btn secondary accept-all \" name=\"agree\" value=\"agree\">Alle akzeptieren</button>\r\n        <button type=\"submit\" class=\"btn secondary reject-all\" name=\"reject\" value=\"reject\">Alle ablehnen</button>\r\n    <a href=\"/v2/partners?sessionId=3_cc-session_41bb06e7-1410-408d-9312-8fdb41fc92a2\" class=\"btn secondary mng-btn manage-settings\" role=\"button\">Datenschutzeinstellungen verwalten</a>\r\n                    </div>"),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(""),
				});
		_mockHttpClientFactory.Setup(e => e.CreateClient(It.IsAny<string>())).Returns(new HttpClient(_mockHandler.Object));

		var manager = new YahooSessionManager(
				_mockHttpClientFactory.Object,
				_mockLogger.Object,
				_mockYahooSessionState.Object,
				_mockPolicyRegistry.Object);

		// Act
		Assert.ThrowsAsync<FinanceNetException>(async () => await manager.RefreshSessionAsync());
	}
}

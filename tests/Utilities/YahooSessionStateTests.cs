using System;
using Finance.Net.Utilities;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Finance.Net.Tests.Utilities;

[TestFixture]
[Category("Unit")]
public class YahooSessionStateTests
{
	private Mock<IOptions<FinanceNetConfiguration>> _mockOptions;

	[SetUp]
	public void SetUp()
	{
		_mockOptions = new Mock<IOptions<FinanceNetConfiguration>>();
		_mockOptions.Setup(x => x.Value).Returns(new FinanceNetConfiguration());
	}

	[Test]
	public void Constructor_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => new YahooSessionState(null));
	}

	[Test]
	public void GetUserAgent_Random_ReturnsValid()
	{
		// Arrange
		var yahooSessionState = new YahooSessionState(_mockOptions.Object);

		// Act
		var result = yahooSessionState.GetUserAgent();

		// Assert
		Assert.That(result, Is.Not.Empty);
	}

	[Test]
	public void GetCrumb_Initialized_ReturnsSame()
	{
		// Arrange
		var yahooSessionState = new YahooSessionState(_mockOptions.Object);

		// Act
		yahooSessionState.SetCrumb("Test", DateTime.UtcNow);
		var result = yahooSessionState.GetCrumb();

		// Assert
		Assert.That(result, Is.EqualTo("Test"));
	}

	[Test]
	public void IsValid_WithValidCookies_ReturnsTrue()
	{
		// Arrange
		var yahooSessionState = new YahooSessionState(_mockOptions.Object);
		var cookie = new System.Net.Cookie("cookieName", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		};
		yahooSessionState.GetCookieContainer().Add(cookie);
		yahooSessionState.SetCrumb("Crumb", DateTime.UtcNow);

		// Act
		var result = yahooSessionState.IsValid();

		// Assert
		Assert.That(result, Is.EqualTo(true));
	}

	[Test]
	public void IsValid_WithOldCookies_ReturnsFalse()
	{
		// Arrange
		var yahooSessionState = new YahooSessionState(_mockOptions.Object);
		var cookie = new System.Net.Cookie("cookieName", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(1)
		};
		yahooSessionState.GetCookieContainer().Add(cookie);
		yahooSessionState.SetCrumb("Crumb", DateTime.UtcNow.AddDays(-1));

		// Act
		var result = yahooSessionState.IsValid();

		// Assert
		Assert.That(result, Is.EqualTo(false));
	}

	[Test]
	public void IsValid_WithExpiredCookies_ReturnsFalse()
	{
		// Arrange
		var yahooSessionState = new YahooSessionState(_mockOptions.Object);
		var cookie = new System.Net.Cookie("cookieName", "Value", "/", ".yahoo.com")
		{
			Expires = DateTime.Now.AddDays(-1)
		};
		yahooSessionState.GetCookieContainer().Add(cookie);
		yahooSessionState.SetCrumb("Crumb", DateTime.UtcNow);

		// Act
		var result = yahooSessionState.IsValid();

		// Assert
		Assert.That(result, Is.EqualTo(false));
	}
}

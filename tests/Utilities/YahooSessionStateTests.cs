using System;
using Finance.Net.Utilities;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Finance.Net.Tests.Utilities;

[TestFixture]
[Category("UnitTests")]
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
        yahooSessionState.SetCrumb("Test");
        var result = yahooSessionState.GetCrumb();

        // Assert
        Assert.That(result, Is.EqualTo("Test"));
    }

    [Test]
    public void InvalidateSession_Initialized_ClearsCookies()
    {
        // Arrange
        var yahooSessionState = new YahooSessionState(_mockOptions.Object);

        // Act
        yahooSessionState.GetCookieContainer().Add(new System.Net.Cookie("cookieName", "Value", "/", "www.test.com"));
        var cookiesCnt = yahooSessionState.GetCookieContainer().Count;
        Assert.That(cookiesCnt, Is.GreaterThanOrEqualTo(1));
        yahooSessionState.InvalidateSession();

        // Assert
        cookiesCnt = yahooSessionState.GetCookieContainer().Count;
        Assert.That(cookiesCnt, Is.EqualTo(0));
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
        yahooSessionState.SetCrumb("Crumb");

        // Act
        var result = yahooSessionState.IsValid();

        // Assert
        Assert.That(true, Is.EqualTo(result));
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
        yahooSessionState.SetCrumb("Crumb");

        // Act
        var result = yahooSessionState.IsValid();

        // Assert
        Assert.That(false, Is.EqualTo(result));
    }
}

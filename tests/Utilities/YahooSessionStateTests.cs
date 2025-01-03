using System;
using Finance.Net.Utilities;
using NUnit.Framework;

namespace Finance.Net.Tests.Utilities;

[TestFixture]
[Category("Unit")]
public class YahooSessionStateTests
{
    [Test]
    public void GetUserAgent_Random_ReturnsValid()
    {
        // Arrange
        var yahooSessionState = new YahooSessionState();

        // Act
        var result = yahooSessionState.GetUserAgent();

        // Assert
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetCrumb_Initialized_ReturnsSame()
    {
        // Arrange
        var yahooSessionState = new YahooSessionState();

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
        var yahooSessionState = new YahooSessionState();
        var cookie = new System.Net.Cookie("cookieName", "Value", "/", ".yahoo.com")
        {
            Expires = DateTime.UtcNow.AddDays(1)
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
        var yahooSessionState = new YahooSessionState();
        var cookie = new System.Net.Cookie("cookieName", "Value", "/", ".yahoo.com")
        {
            Expires = DateTime.UtcNow.AddDays(1)
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
        var yahooSessionState = new YahooSessionState();
        var cookie = new System.Net.Cookie("cookieName", "Value", "/", ".yahoo.com")
        {
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        yahooSessionState.GetCookieContainer().Add(cookie);
        yahooSessionState.SetCrumb("Crumb", DateTime.UtcNow);

        // Act
        var result = yahooSessionState.IsValid();

        // Assert
        Assert.That(result, Is.EqualTo(false));
    }
}

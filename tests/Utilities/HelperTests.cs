﻿using System;
using AutoMapper;
using Finance.Net.Enums;
using Finance.Net.Mappings;
using Finance.Net.Models.Yahoo.Dtos;
using Finance.Net.Utilities;
using NUnit.Framework;

namespace Finance.Net.Tests.Utilities;

[TestFixture]
[Category("Unit")]
public class HelperTests
{
    [TestCase(null, true)]
    [TestCase(new string[] { }, true)]
    [TestCase(new[] { "" }, false)]
    [TestCase(new[] { "Abc", "Abc" }, false)]
    public void IsNullOrEmpty_WithValidEntries_ReturnsResult(string[] array, bool expected)
    {
        var result = array.IsNullOrEmpty();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(2024, 11, 1, 1_730_419_200)]
    [TestCase(2011, 11, 27, 1_322_352_000)]
    [TestCase(1972, 11, 27, 91_670_400)]
    [TestCase(null, null, null, null)]
    public void ToUnixTime_WithValidEntries_ReturnsResult(int? year, int? month, int? day, long? expected)
    {
        // Arrange
        var dateTime = expected == null ? (DateTime?)null : new DateTime(year ?? 0, month ?? 0, day ?? 0, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = Helper.ToUnixTime(dateTime);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(1_733_443_200, 2024, 12, 6)]
    [TestCase(1_233_446_400, 2009, 2, 1)]
    [TestCase(null, null, null, null)]
    public void UnixToDateTime_WithValidEntries_ReturnsResult(long? unixTimeSeconds, int? expectedYear, int? expectedMonth, int? expectedDay)
    {
        // Arrange
        var expected = expectedYear == null ? (DateTime?)null : new DateTime(expectedYear ?? 0, expectedMonth ?? 0, expectedDay ?? 0, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = Helper.UnixToDateTime(unixTimeSeconds);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(1_733_443_200_000, 2024, 12, 6)]
    [TestCase(1_233_446_400_000, 2009, 2, 1)]
    [TestCase(null, null, null, null)]
    public void UnixMillisecsToDate_WithValidEntries_ReturnsResult(long? unixTimeMilliseconds, int? expectedYear, int? expectedMonth, int? expectedDay)
    {
        var expected = unixTimeMilliseconds == null ? (DateTime?)null : new DateTime(expectedYear ?? 0, expectedMonth ?? 0, expectedDay ?? 0, 0, 0, 0, DateTimeKind.Utc);

        var result = Helper.UnixMillisecsToDate(unixTimeMilliseconds);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("100", 100)]
    [TestCase("-100", -100)]
    [TestCase("1,200", 1200)]
    [TestCase("1.1k", 1100)]
    [TestCase("-1.1k", -1100)]
    [TestCase("1.1Mio", 1_100_000)]
    [TestCase("1.1Mrd", 1_100_000_000)]
    [TestCase("1.1Bio", 1_100_000_000_000)]
    [TestCase("1.1Trl", 1_100_000_000_000_000)]
    [TestCase("---", null)]
    [TestCase(null, null)]
    public void ParseLong_WithValidEntries_ReturnsResult(string numberString, long? expected)
    {
        var result = Helper.ParseLong(numberString);

        Assert.That(result, Is.EqualTo(expected));
    }
    [TestCase("1.0.0")]
    [TestCase("-,100.")]
    [TestCase("1.1ku")]
    [TestCase("-1.1koi")]
    [TestCase("1.1Mioll")]
    [TestCase("1.1Mrdo")]
    [TestCase("1.1Biola")]
    [TestCase("1.1Trlilll")]
    public void ParseLong_WithInvalidEntries_Throws(string numberString)
    {
        Assert.Throws<FormatException>(() => Helper.ParseLong(numberString));
    }

    [TestCase("-10", -10)]
    [TestCase("-10.1", -10.1)]
    [TestCase("1,234.12", 1234.12)]
    [TestCase("1.1k", 1100)]
    [TestCase("---", null)]
    [TestCase(null, null)]
    public void ParseDecimal_WithValidEntries_ReturnsResult(string numberString, decimal? expected)
    {
        var result = Helper.ParseDecimal(numberString);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("-+10")]
    [TestCase("-1.,0.1")]
    [TestCase("1,2.34.12")]
    [TestCase("1.1klb")]
    [TestCase("-1--")]
    [TestCase("-1.9,0.1")]
    [TestCase("1-00Mega")]
    public void ParseDecimal_WithInvalidEntries_Throws(string numberString)
    {
        Assert.Throws<FormatException>(() => Helper.ParseDecimal(numberString));
    }

    [TestCase("Nov 1, 2024", 2024, 11, 1)]
    [TestCase("Nov 01, 2024", 2024, 11, 1)]
    [TestCase("November 1, 2024", 2024, 11, 1)]
    [TestCase("November 01, 2024", 2024, 11, 1)]
    [TestCase("2024-11-1", 2024, 11, 1)]
    [TestCase("2024-11-01", 2024, 11, 1)]
    [TestCase("2024/11/1", 2024, 11, 1)]
    [TestCase("2024/11/01", 2024, 11, 1)]
    public void ParseDate_WithValidEntries_ReturnsResult(string dateString, int expectedYear, int expectedMonth, int expectedDay)
    {
        // Arrange
        var expected = new DateTime(expectedYear, expectedMonth, expectedDay, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = Helper.ParseDate(dateString);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ParseDate_WithInvalidNumber_Throws()
    {
        Assert.Throws<FormatException>(() => Helper.ParseDate("invalid"));
    }

    [TestCase(EInterval.Interval_1Min, "1min")]
    [TestCase(EInterval.Interval_5Min, "5min")]
    [TestCase(EInterval.Interval_15Min, "15min")]
    [TestCase(EInterval.Interval_30Min, "30min")]
    [TestCase(EInterval.Interval_60Min, "60min")]
    public void Description_AllIntervals_ReturnsText(EInterval intervall, string expected)
    {
        // Arrange
        // Act
        var result = intervall.GetDescription();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Minify_WithWhiteChars_ReturnsText()
    {
        // Arrange
        const string str = "Hello\n\nWorld\r\n\t!";

        // Act
        var result = str.Minify();

        // Assert
        Assert.That(result, Is.EqualTo("Hello World !"));
    }

    [Test]
    public void Minify_WithNullObj_ReturnsNull()
    {
        // Arrange
        const string str = null;

        // Act
        var result = str.Minify();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void AreAllFieldsNull_AllNull_ReturnsTrue()
    {
        // Arrange
        var model = new QuoteResponse();

        // Act
        var result = Helper.AreAllPropertiesNull(model);

        // Assert
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void AreAllFieldsNull_NotNull_ReturnsFalse()
    {
        // Arrange
        var model = new QuoteResponse
        {
            AskSize = 123
        };

        // Act
        var result = Helper.AreAllPropertiesNull(model);

        // Assert
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void AreAllFieldsNull_ObjNull_ReturnsTrue()
    {
        // Arrange
        // Act
        var result = Helper.AreAllPropertiesNull((QuoteResponse)null);

        // Assert
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    [TestCase(null, null)]
    [TestCase("", null)]
    [TestCase("Example Text (ABC)", "Example Text")]
    [TestCase("No Header", "No Header")]
    [TestCase("Trailing Spaces (Header)  ", "Trailing Spaces")]
    [TestCase("Special Characters !@#$%^&*() (Header)", "Special Characters !@#$%^&*()")]
    [TestCase("String with (Header)", "String with")]
    [TestCase("String with timeout (Header)", "String with timeout")]
    public void RemoveSymbolHeader_ShouldRemoveHeader_WhenValidInputProvided(string input, string expected)
    {
        // Act
        var result = Helper.RemoveSymbolHeader(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void RemoveSymbolHeader_ShouldThrowTimeoutException_WhenRegexTimeoutExceeds()
    {
        // Arrange
        var longInput = new string('a', 10000) + " (Header)";

        // Act & Assert
        Assert.DoesNotThrow(() => Helper.RemoveSymbolHeader(longInput), "Regex should not throw due to timeout.");
    }

    [Test]
    public void Automapper_AddProfile_IsValid()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapMethod = m => false;
            cfg.AddProfile<YahooQuoteAutomapperProfile>();
        });

        // This will throw an exception if the configuration is invalid
        config.AssertConfigurationIsValid();

        Assert.Pass("AutoMapper configuration is valid.");
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Finance.Net.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Finance.Net.Tests.IntegrationTests;

[TestFixture]
[Category("Integration")]
public class YahooFinanceTests
{
    private static IServiceProvider s_serviceProvider;
    private IYahooFinanceService _service;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        s_serviceProvider = TestHelper.SetUpServiceProvider();
        _service = s_serviceProvider.GetRequiredService<IYahooFinanceService>();
    }

    [TearDown]
    public void TearDown()
    {
        Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult(); // 2 secs between runs
    }

    [TestCase(EInstrumentType.Index, 20)]
    [TestCase(EInstrumentType.Stock, 100)]
    [TestCase(EInstrumentType.ETF, 200)]
    [TestCase(EInstrumentType.Forex, 20)]
    [TestCase(EInstrumentType.Crypto, 200)]
    [TestCase(null, 500)]
    public async Task GetSymbolsAsync_Success(EInstrumentType? type, int expectedCnt)
    {
        var symbols = await _service.GetSymbolsAsync(type);

        Assert.That(symbols, Is.Not.Empty);
        Assert.That(symbols.Count, Is.GreaterThanOrEqualTo(expectedCnt));
        Assert.That(symbols.All(e => !string.IsNullOrEmpty(e.Symbol)));
        Assert.That(symbols.Any(e => e.InstrumentType != null));

        Assert.Pass($"cnt {symbols.Count()}");
    }

    [Test]
    public async Task GetRecordsAsync_WithDividend_Success()
    {
        var startDate = new DateTime(2020, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        var records = await _service.GetRecordsAsync("SAP.DE", startDate);

        Assert.That(records, Is.Not.Empty);

        var lastRecentRecord = records.FirstOrDefault();
        Assert.That(lastRecentRecord.Date.Date <= DateTime.UtcNow, Is.True);
        Assert.That(lastRecentRecord.Date.Date >= startDate, Is.True);
    }

    [Test]
    public async Task GetProfileAsync_StaticInstance_ReturnsProfile()
    {
        var service = YahooFinanceService.Create();
        var profile = await service.GetProfileAsync("SAP.DE");

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.Adress, Is.Not.Null);
    }

    [TestCase("IBM", true)]       // IBM (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("8058.T", true)]    // Mitsubishi (Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", true)]  // Euro to USD
    [TestCase("TESTING.NET", false)]
    [TestCase("BTC-USD", true)]   // Bitcoin - USD (Crypto)
    public async Task GetQuoteAsync_ValidSymbols_ReturnsQuote(string symbol, bool shouldHaveQuote)
    {
        if (shouldHaveQuote)
        {
            var quote = await _service.GetQuoteAsync(symbol);

            Assert.That(quote, Is.Not.Null);
            Assert.That(quote.Symbol, Is.EqualTo(symbol));
            Assert.That(quote.FirstTradeDate.Value.Date >= new DateTime(1920, 1, 1) && quote.FirstTradeDate.Value.Date <= DateTime.UtcNow, Is.True);
            Assert.That(!string.IsNullOrWhiteSpace(quote.QuoteType), Is.True);
            Assert.That(!string.IsNullOrWhiteSpace(quote.Exchange), Is.True);
            Assert.That(!string.IsNullOrWhiteSpace(quote.ShortName), Is.True);
            Assert.That(!string.IsNullOrWhiteSpace(quote.LongName), Is.True);
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetQuoteAsync(symbol));
        }
    }

    [TestCase("BTC-USD", true)]   // Bitcoin - USD (Crypto)
    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("6758.T", true)]     // Sony  (Tokyo)
    [TestCase("VOO", false)]       // Vanguard S&P 500 (ETF)
    [TestCase("EURUSD=X", false)]  // Euro to USD (Forex)
    public async Task GetProfileAsync_ValidSymbols_ReturnsProfile(string symbol, bool shouldHaveProfile)
    {
        if (shouldHaveProfile)
        {
            var profile = await _service.GetProfileAsync(symbol);

            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.Name, Is.Not.Null);

            if (symbol != "BTC-USD")
            {
                Assert.That(profile.Industry, Is.Not.Null);
                Assert.That(profile.Sector, Is.Not.Null);
                Assert.That(profile.Phone, Is.Not.Null);
                Assert.That(profile.CorporateGovernance, Is.Not.Null);
                Assert.That(profile.CntEmployees, Is.Not.Null);
                Assert.That(profile.Adress, Is.Not.Null);
                Assert.That(profile.Description, Is.Not.Null);
                Assert.That(profile.Website, Is.Not.Null);
            }
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetProfileAsync(symbol));
        }
    }

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", true)]       // SAP SE (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("8058.T", true)]    // Mitsubishi (Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", true)]  // Euro to USD
    [TestCase("BTC-USD", true)]   // Bitcoin - USD (Crypto)
    [TestCase("TESTING.NET", false)]
    public async Task GetRecordsAsync_ValidSymbols_ReturnsRecords(string symbol, bool shouldHaveRecords)
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        if (shouldHaveRecords)
        {
            var records = await _service.GetRecordsAsync(symbol, startDate);

            Assert.That(records, Is.Not.Empty);

            var lastRecentRecord = records.FirstOrDefault();
            Assert.That(lastRecentRecord.Date.Date <= DateTime.UtcNow, Is.True);
            Assert.That(lastRecentRecord.Date.Date >= startDate, Is.True);
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetRecordsAsync(symbol, startDate));
        }
    }

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", true)]       // SAP SE (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("8058.T", true)]    // Mitsubishi (Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", true)]  // Euro to USD
    [TestCase("BTC-USD", true)]   // Bitcoin - USD (Crypto)
    [TestCase("TESTING.NET", false)]
    public async Task GetSummaryAsync_ValidSymbols_ReturnsSummary(string symbol, bool shouldHaveSummary)
    {
        if (shouldHaveSummary)
        {
            var summary = await _service.GetSummaryAsync(symbol);
            Assert.That(summary, Is.Not.Null);
            Assert.That(summary.PreviousClose, Is.Not.Null);
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetSummaryAsync(symbol));
        }
    }

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("IBM", true)]       // IBM (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("8058.T", true)]    // Mitsubishi (Tokyo)
    [TestCase("VOO", false)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", false)]  // Euro to USD
    public async Task GetFinancialReportsAsync_ValidSymbols_ReturnsReports(string symbol, bool shouldHaveReport)
    {
        if (shouldHaveReport)
        {
            var reports = await _service.GetFinancialReportsAsync(symbol);
            Assert.That(reports, Is.Not.Empty);

            var firstReport = reports.First();
            Assert.That(!string.IsNullOrWhiteSpace(firstReport.Key));
            Assert.That(firstReport.Value.TotalRevenue > 0);
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetFinancialReportsAsync(symbol));
        }
    }
}

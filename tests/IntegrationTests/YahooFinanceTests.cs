using System;
using System.Linq;
using System.Threading.Tasks;
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

    [Test]
    public async Task GetHistoryRecordsAsync_WithDividend_Success()
    {
        var startDate = new DateTime(2020, 01, 01);
        var records = await _service.GetHistoryRecordsAsync("SAP.DE", startDate);

        Assert.That(records, Is.Not.Empty);

        var lastRecentRecord = records.FirstOrDefault();
        Assert.That(lastRecentRecord.Date.Date <= DateTime.UtcNow, Is.True);
        Assert.That(lastRecentRecord.Date.Date >= startDate, Is.True);
    }

    [Test]
    public async Task GetProfileAsync_WithoutIoC_ReturnsProfile()
    {
        var service = YahooFinanceService.Create();
        var profile = await service.GetProfileAsync("SAP.DE");

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.Adress, Is.Not.Null);
    }

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("IBM", true)]       // IBM (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("6758.T", true)]    // Sony Group Corporation (Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", true)]  // Euro to USD
    [TestCase("TESTING.NET", false)]
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

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", true)]       // SAP SE (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("6758.T", true)]    // Sony Group Corporation (Tokyo)
    [TestCase("VOO", false)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", false)]  // Euro to USD
    public async Task GetProfileAsync_ValidSymbols_ReturnsProfile(string symbol, bool shouldHaveProfile)
    {
        if (shouldHaveProfile)
        {
            var profile = await _service.GetProfileAsync(symbol);

            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.Industry, Is.Not.Null);
            Assert.That(profile.Sector, Is.Not.Null);
            Assert.That(profile.Phone, Is.Not.Null);
            Assert.That(profile.CorporateGovernance, Is.Not.Null);
            Assert.That(profile.CntEmployees, Is.Not.Null);
            Assert.That(profile.Adress, Is.Not.Null);
            Assert.That(profile.Description, Is.Not.Null);
            Assert.That(profile.Website, Is.Not.Null);
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetProfileAsync(symbol));
        }
    }

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", true)]       // SAP SE (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("6758.T", true)]    // Sony Group Corporation (Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", true)]  // Euro to USD
    [TestCase("TESTING.NET", false)]
    public async Task GetHistoryRecordsAsync_ValidSymbols_ReturnsRecords(string symbol, bool shouldHaveRecords)
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        if (shouldHaveRecords)
        {
            var records = await _service.GetHistoryRecordsAsync(symbol, startDate);

            Assert.That(records, Is.Not.Empty);

            var lastRecentRecord = records.FirstOrDefault();
            Assert.That(lastRecentRecord.Date.Date <= DateTime.UtcNow, Is.True);
            Assert.That(lastRecentRecord.Date.Date >= startDate, Is.True);
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetHistoryRecordsAsync(symbol, startDate));
        }
    }

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", true)]       // SAP SE (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("6758.T", true)]    // Sony Group Corporation (Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", true)]  // Euro to USD
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
    [TestCase("6758.T", true)]    // Sony Group Corporation (Tokyo)
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

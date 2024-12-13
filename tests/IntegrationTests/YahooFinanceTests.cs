using System;
using System.Linq;
using System.Threading.Tasks;
using Finance.Net.Interfaces;
using Finance.Net.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Finance.Net.Tests.IntegrationTests;

[TestFixture]
[Category("IntegrationTests")]
public class YahooFinanceTests
{
    private static IServiceProvider _serviceProvider;
    private IYahooFinanceService _service;

    [SetUp]
    public void SetUp()
    {
        _serviceProvider = TestHelper.SetUpServiceProvider();
        _service = _serviceProvider.GetRequiredService<IYahooFinanceService>();
    }

    [TearDown]
    public void TearDown()
    {
        Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult(); // 2 secs between runs
    }

    [Test]
    public async Task GetHistoricalRecordsAsync_WithDividend_Success()
    {
        var startDate = new DateTime(2020, 01, 01);
        var records = await _service.GetHistoricalRecordsAsync("SAP.DE", startDate);

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

    [TestCase("MSFT")]      // Microsoft Corporation (Nasdaq)
    [TestCase("IBM")]       // IBM (Nasdaq)
    [TestCase("SAP.DE")]    // SAP SE (Xetra)
    [TestCase("6758.T")]    // Sony Group Corporation (Tokyo)
    [TestCase("VOO")]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X")]  // Euro to USD
    public async Task GetQuoteAsync_ValidSymbols_ReturnsQuote(string symbol)
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

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", true)]       // SAP SE (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("6758.T", true)]    // Sony Group Corporation (Tokyo)
    [TestCase("VOO", false)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", false)]  // Euro to USD
    public async Task GetProfileAsync_ValidSymbols_ReturnsProfile(string symbol, bool shouldHaveProfile)
    {
        var profile = await _service.GetProfileAsync(symbol);

        if (shouldHaveProfile)
        {
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
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.Industry, Is.Null);
            Assert.That(profile.Sector, Is.Null);
            Assert.That(profile.Phone, Is.Null);
            Assert.That(profile.CorporateGovernance, Is.Null);
            Assert.That(profile.CntEmployees, Is.Null);
            Assert.That(profile.Adress, Is.Null);
            Assert.That(profile.Description, Is.Null);
            Assert.That(profile.Website, Is.Null);
        }
    }

    [TestCase("MSFT")]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP")]       // SAP SE (Nasdaq)
    [TestCase("SAP.DE")]    // SAP SE (Xetra)
    [TestCase("6758.T")]    // Sony Group Corporation (Tokyo)
    [TestCase("VOO")]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X")]  // Euro to USD
    public async Task GetHistoricalRecordsAsync_ValidSymbols_ReturnsRecords(string symbol)
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var records = await _service.GetHistoricalRecordsAsync(symbol, startDate);

        Assert.That(records, Is.Not.Empty);

        var lastRecentRecord = records.FirstOrDefault();
        Assert.That(lastRecentRecord.Date.Date <= DateTime.UtcNow, Is.True);
        Assert.That(lastRecentRecord.Date.Date >= startDate, Is.True);
    }

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", true)]       // SAP SE (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("6758.T", true)]    // Sony Group Corporation (Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 ETF
    [TestCase("EURUSD=X", true)]  // Euro to USD
    public async Task GetSummaryAsync_ValidSymbols_ReturnsSummary(string symbol, bool shouldHaveSummary)
    {
        var summary = await _service.GetSummaryAsync(symbol);

        if (shouldHaveSummary)
        {
            Assert.That(summary, Is.Not.Null);
            Assert.That(summary.PreviousClose, Is.Not.Null);
        }
        else
        {
            Assert.That(summary, Is.Null);
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
        var reports = await _service.GetFinancialReportsAsync(symbol);

        if (shouldHaveReport)
        {
            Assert.That(reports, Is.Not.Empty);

            var firstReport = reports.First();
            Assert.That(!string.IsNullOrWhiteSpace(firstReport.Key));
            Assert.That(firstReport.Value.TotalRevenue > 0);
        }
        else
        {
            Assert.That(reports, Is.Null.Or.Empty);
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Finance.Net.Tests.IntegrationTests;

[TestFixture]
[Category("Integration")]
public class YahooFinanceTests
{
    private IYahooFinanceService _service;
    private ServiceProvider _serviceProvider;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _serviceProvider = TestHelper.SetUpServiceProvider();
        _service = _serviceProvider.GetRequiredService<IYahooFinanceService>();
    }

    [TearDown]
    public void TearDown()
    {
        Task.Delay(TimeSpan.FromSeconds(4)).GetAwaiter().GetResult();
    }

    [TestCase(EAssetType.Index, 20)]
    [TestCase(EAssetType.Stock, 20)]
    [TestCase(EAssetType.ETF, 20)]
    [TestCase(EAssetType.Forex, 20)]
    [TestCase(EAssetType.Crypto, 20)]
    [TestCase(null, 100)]
    public async Task GetInstrumentsAsync(EAssetType? type, int expectedCnt)
    {
        var symbols = await _service.GetInstrumentsAsync(type);

        Assert.That(symbols, Is.Not.Empty);
        Assert.That(symbols.Count, Is.GreaterThanOrEqualTo(expectedCnt));
        Assert.That(symbols.All(e => !string.IsNullOrEmpty(e.Symbol)));
        Assert.That(symbols.Any(e => e.InstrumentType != null));

        Assert.Pass($"cnt = {symbols.Count()}");
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

    [TestCase("AAPL")]
    [TestCase("TSLA")]

    public async Task GetInstrumentsAsync(string symbol)
    {
        var instruments = await _service.GetInstrumentsAsync(EAssetType.Stock);

        var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);
        Assert.That(instrument, Is.Not.Null);
        Assert.That(instrument.Symbol, Is.Not.Empty);

        Assert.Pass(JsonConvert.SerializeObject(instrument));
    }

    [TestCase("TSLA", true)]      // Tesla (Stock - Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Stock - Xetra)
    [TestCase("8058.T", true)]    // Mitsubishi (Stock - Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 (ETF)
    [TestCase("EURUSD=X", true)]  // Euro to USD (Forex)
    [TestCase("BTC-USD", true)]   // Bitcoin - USD (Crypto)
    [TestCase("^GSPC", true)]     // S&P 500 (Index)
    [TestCase("TESTING.NET", false)]
    public async Task GetRecordsAsync(string symbol, bool shouldHaveRecords)
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        if (shouldHaveRecords)
        {
            var records = await _service.GetRecordsAsync(symbol, startDate);

            Assert.That(records, Is.Not.Empty);
            Assert.That(records.All(e => e.Open != null), Is.True);

            var lastRecentRecord = records.FirstOrDefault();
            Assert.That(lastRecentRecord.Date.Date <= DateTime.UtcNow, Is.True);
            Assert.That(lastRecentRecord.Date.Date >= startDate, Is.True);

            Assert.Pass(JsonConvert.SerializeObject(lastRecentRecord));
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetRecordsAsync(symbol, startDate));
        }
    }

    [TestCase("TSLA", true)]      // Tesla (Stock - Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Stock - Xetra)
    [TestCase("8058.T", true)]    // Mitsubishi (Stock - Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 (ETF)
    [TestCase("EURUSD=X", true)]  // Euro to USD (Forex)
    [TestCase("BTC-USD", true)]   // Bitcoin - USD (Crypto)
    [TestCase("^GSPC", true)]     // S&P 500 (Index)
    [TestCase("TESTING.NET", false)]
    public async Task GetQuoteAsync(string symbol, bool shouldHaveQuote)
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

            Assert.Pass(JsonConvert.SerializeObject(quote));
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetQuoteAsync(symbol));
        }
    }

    [TestCase("TSLA", true)]      // Tesla (Stock - Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Stock - Xetra)
    [TestCase("8058.T", true)]    // Mitsubishi (Stock - Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 (ETF)
    [TestCase("EURUSD=X", false)]  // Euro to USD (Forex)
    [TestCase("BTC-USD", true)]   // Bitcoin - USD (Crypto)
    [TestCase("^GSPC", false)]     // S&P 500 (Index)
    [TestCase("TESTING.NET", false)]
    public async Task GetProfileAsync(string symbol, bool shouldHaveData)
    {
        if (shouldHaveData)
        {
            var profile = await _service.GetProfileAsync(symbol);

            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.Description, Is.Not.Empty);

            if (symbol != "BTC-USD")
            {
                Assert.That(profile.Industry, Is.Not.Empty);
                Assert.That(profile.Sector, Is.Not.Empty);
                Assert.That(profile.Phone, Is.Not.Empty);
                Assert.That(profile.Adress, Is.Not.Empty);
                Assert.That(profile.Website, Is.Not.Empty);
            }

            Assert.Pass(JsonConvert.SerializeObject(profile));
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetProfileAsync(symbol));
        }
    }


    [TestCase("TSLA", true)]      // Tesla (Stock - Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Stock - Xetra)
    [TestCase("8058.T", true)]    // Mitsubishi (Stock - Tokyo)
    [TestCase("VOO", true)]       // Vanguard S&P 500 (ETF)
    [TestCase("EURUSD=X", true)]  // Euro to USD (Forex)
    [TestCase("BTC-USD", true)]   // Bitcoin - USD (Crypto)
    [TestCase("^GSPC", true)]     // S&P 500 (Index)
    [TestCase("TESTING.NET", false)]
    public async Task GetSummaryAsync(string symbol, bool shouldHaveData)
    {
        if (shouldHaveData)
        {
            var summary = await _service.GetSummaryAsync(symbol);

            Assert.That(summary, Is.Not.Null);

            Assert.That(summary.Name, Is.Not.Empty);
            Assert.That(summary.PreviousClose, Is.Not.Null);

            Assert.Pass(JsonConvert.SerializeObject(summary));
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetSummaryAsync(symbol));
        }
    }

    [TestCase("TSLA", true)]      // Tesla (Stock - Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Stock - Xetra)
    [TestCase("8058.T", true)]    // Mitsubishi (Stock - Tokyo)
    [TestCase("VOO", false)]       // Vanguard S&P 500 (ETF)
    [TestCase("EURUSD=X", false)]  // Euro to USD (Forex)
    [TestCase("BTC-USD", false)]   // Bitcoin - USD (Crypto)
    [TestCase("^GSPC", false)]     // S&P 500 (Index)
    [TestCase("TESTING.NET", false)]
    public async Task GetFinancialsAsync(string symbol, bool shouldHaveData)
    {
        if (shouldHaveData)
        {
            var financials = await _service.GetFinancialsAsync(symbol);

            Assert.That(financials, Is.Not.Empty);

            var firstReport = financials.FirstOrDefault();
            Assert.That(!string.IsNullOrWhiteSpace(firstReport.Key));
            Assert.That(firstReport.Value.TotalRevenue > 0);

            Assert.Pass(JsonConvert.SerializeObject(financials));
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetFinancialsAsync(symbol));
        }
    }
}

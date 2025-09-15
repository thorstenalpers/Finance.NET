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
public class AlphaVantageTests
{
    private IAlphaVantageService _service;

    [SetUp]
    public void SetUp()
    {
        var serviceProvider = TestHelper.SetUpServiceProvider();
        _service = serviceProvider.GetRequiredService<IAlphaVantageService>();
    }

    [TestCase("TSLA", true)]      // Tesla (Stock - Nasdaq)
    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", true)]       // SAP SE (Nasdaq)
    [TestCase("GOOG", true)]      // Alphabet (Nasdaq)
    [TestCase("TESTING.NET", false)]
    public async Task GetOverviewAsync(string symbol, bool shouldHave)
    {
        if (shouldHave)
        {
            var overview = await _service.GetOverviewAsync(symbol);
            Assert.That(overview, Is.Not.Null);
            Assert.That(overview.Symbol, Is.EqualTo(symbol));
            Assert.That(overview.Industry, Is.Not.Empty);
            Assert.Pass(JsonConvert.SerializeObject(overview));
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetOverviewAsync(symbol));
        }
    }

    [TestCase("TSLA", true)]      // Tesla (Stock - Nasdaq)
    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", true)]       // SAP SE (Nasdaq)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("VOO", true)]       // Vanguard S&P 500 ETF
    [TestCase("TESTING.NET", false)]
    public async Task GetRecordsAsync(string symbol, bool shouldHave)
    {
        if (shouldHave)
        {
            var records = await _service.GetRecordsAsync(symbol, DateTime.UtcNow.AddDays(-7));

            Assert.That(records, Is.Not.Empty);
            Assert.Pass(JsonConvert.SerializeObject(records.LastOrDefault()));
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetRecordsAsync(symbol, DateTime.UtcNow.AddDays(-7)));
        }
    }

    [TestCase("TSLA", EInterval.Interval_15Min, true)]      // Tesla (Stock - Nasdaq)
    [TestCase("MSFT", EInterval.Interval_15Min, true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("SAP", EInterval.Interval_1Min, true)]       // SAP SE (Nasdaq)
    [TestCase("SAP", EInterval.Interval_5Min, true)]       // SAP SE (Nasdaq)
    [TestCase("SAP", EInterval.Interval_15Min, true)]       // SAP SE (Nasdaq)
    [TestCase("SAP", EInterval.Interval_30Min, true)]       // SAP SE (Nasdaq)
    [TestCase("SAP", EInterval.Interval_60Min, true)]       // SAP SE (Nasdaq)
    [TestCase("TESTING.NET", EInterval.Interval_15Min, false)]
    public async Task GetIntradayRecordsAsync(string symbol, EInterval eInterval, bool shouldHave)
    {
        var startDay = new DateTime(2024, 12, 02, 0, 0, 0, DateTimeKind.Utc);
        var endDay = new DateTime(2024, 12, 02, 0, 0, 0, DateTimeKind.Utc);
        if (shouldHave)
        {
            var records = await _service.GetIntradayRecordsAsync(symbol, startDay, endDay, eInterval);
            Assert.That(records, Is.Not.Empty);
            Assert.Pass(JsonConvert.SerializeObject(records.LastOrDefault()));
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetIntradayRecordsAsync(symbol, startDay, endDay, eInterval));
        }
    }

    [TestCase("EUR", "USD", true)]
    [TestCase("TESTING1.NET", "TESTING2.NET", false)]
    public async Task GetForexRecordsAsync(string currency1, string currency2, bool shouldHave)
    {
        if (shouldHave)
        {
            var records = await _service.GetForexRecordsAsync(currency1, currency2, DateTime.UtcNow.AddDays(-3).Date);
            Assert.That(records, Is.Not.Empty);
            Assert.Pass(JsonConvert.SerializeObject(records.LastOrDefault()));
        }
        else
        {
            Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetForexRecordsAsync(currency1, currency2, DateTime.UtcNow.AddDays(-3)));
        }
    }
}

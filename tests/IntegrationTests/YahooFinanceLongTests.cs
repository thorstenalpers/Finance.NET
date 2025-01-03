using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finance.Net.Interfaces;
using Finance.Net.Models.Yahoo;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Finance.Net.Tests.IntegrationTests;

[TestFixture]
[Category("Long-Running")]
public class YahooFinanceLongTests
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

    [TestCase(100)]
    public async Task GetProfileAsync_NTimes_Success(int times)
    {
        Profile firstProfile = null;
        for (var i = 0; i < times; i++)
        {
            Console.WriteLine($"{i + 1})");
            var profile = await _service.GetProfileAsync("IBM");
            Assert.That(profile, Is.Not.Null);

            firstProfile ??= profile;

            Assert.That(profile, Is.EqualTo(firstProfile));
        }
    }

    [TestCase(100)]
    public async Task GetSummaryAsync_NTimes_Success(int times)
    {
        Summary firstSummary = null;
        for (var i = 0; i < times; i++)
        {
            Console.WriteLine($"{i + 1})");
            var summary = await _service.GetSummaryAsync("IBM");
            Assert.That(summary, Is.Not.Null);

            firstSummary ??= summary;

            Assert.That(summary.Name, Is.EqualTo(firstSummary.Name));
            Assert.That(summary.EarningsDate, Is.EqualTo(firstSummary.EarningsDate));
            Assert.That(summary.WeekRange52_Max, Is.EqualTo(firstSummary.WeekRange52_Max));
        }
    }

    [TestCase(100)]
    public async Task GetFinancialsAsync_NTimes_Success(int times)
    {
        FinancialReport firstReport = null;
        for (var i = 0; i < times; i++)
        {
            Console.WriteLine($"{i + 1})");
            var report = await _service.GetFinancialsAsync("IBM");
            Assert.That(report, Is.Not.Empty);

            firstReport ??= report.OrderBy(e => e.Key).First().Value;

            Assert.That(report.OrderBy(e => e.Key).First().Value, Is.EqualTo(firstReport));
        }
    }

    [TestCase(100)]
    public async Task GetRecordsAsync_NTimes_Success(int times)
    {
        List<decimal?> firstRecords = null;
        for (var i = 0; i < times; i++)
        {
            Console.WriteLine($"{i + 1})");
            var records = await _service.GetRecordsAsync("IBM");
            Assert.That(records, Is.Not.Empty);

            firstRecords ??= records.Select(e => e.Open).ToList();

            Assert.That(records.Select(e => e.Open), Is.EquivalentTo(firstRecords));
        }
    }

    [TestCase(100)]
    public async Task GetQuoteAsync_NTimes_Success(int times)
    {
        Quote firstQuote = null;
        for (var i = 0; i < times; i++)
        {
            Console.WriteLine($"{i + 1})");
            var quote = await _service.GetQuoteAsync("IBM");
            Assert.That(quote, Is.Not.Null);

            firstQuote ??= quote;

            Assert.That(quote.DisplayName, Is.EqualTo(firstQuote.DisplayName));
            Assert.That(quote.ShortName, Is.EqualTo(firstQuote.ShortName));
        }
    }
}

using System;
using System.Threading.Tasks;
using Finance.Net.Exceptions;
using Finance.Net.Interfaces;
using Finance.Net.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Finance.Net.Tests.IntegrationTests;

[TestFixture]
[Category("Integration")]
public class AlphaVantageTests
{
	private IServiceProvider _serviceProvider;
	private IAlphaVantageService _service;

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		_serviceProvider = TestHelper.SetUpServiceProvider();
		_service = _serviceProvider.GetRequiredService<IAlphaVantageService>();
	}

	[Test]
	public async Task GetCompanyOverviewAsync_WithoutIoC_ValidSymbols_ReturnsOverview()
	{
		var cfg = _serviceProvider.GetRequiredService<IOptions<FinanceNetConfiguration>>();

		var service = AlphaVantageService.Create(new FinanceNetConfiguration
		{
			AlphaVantageApiKey = cfg.Value.AlphaVantageApiKey
		});

		var overview = await service.GetCompanyOverviewAsync("SAP");

		Assert.That(overview, Is.Not.Null);
		Assert.That(overview.Symbol, Is.EqualTo("SAP"));
	}

	[TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
	[TestCase("SAP", true)]       // SAP SE (Nasdaq)
	[TestCase("GOOG", true)]      // Alphabet (Nasdaq)
	[TestCase("TESTING.NET", false)]
	public async Task GetCompanyOverviewAsync_ValidSymbols_ReturnsOverview(string symbol, bool shouldHave)
	{
		if (shouldHave)
		{
			var overview = await _service.GetCompanyOverviewAsync(symbol);
			Assert.That(overview, Is.Not.Null);
			Assert.That(overview.Symbol, Is.EqualTo(symbol));
		}
		else
		{
			Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetCompanyOverviewAsync(symbol));
		}
	}

	[TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
	[TestCase("SAP", true)]       // SAP SE (Nasdaq)
	[TestCase("SAP.DE", true)]    // SAP SE (Xetra)
	[TestCase("VOO", true)]       // Vanguard S&P 500 ETF
	[TestCase("TESTING.NET", false)]
	public async Task GetHistoryRecordsAsync_ValidSymbols_ReturnsRecords(string symbol, bool shouldHave)
	{
		if (shouldHave)
		{
			var records = await _service.GetHistoryRecordsAsync(symbol, DateTime.UtcNow.AddDays(-7));

			Assert.That(records, Is.Not.Empty);
		}
		else
		{
			Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetHistoryRecordsAsync(symbol, DateTime.UtcNow.AddDays(-7)));
		}
	}

	[TestCase("MSFT", Models.AlphaVantage.EInterval.Interval_15Min, true)]      // Microsoft Corporation (Nasdaq)
	[TestCase("SAP", Models.AlphaVantage.EInterval.Interval_1Min, true)]       // SAP SE (Nasdaq)
	[TestCase("SAP", Models.AlphaVantage.EInterval.Interval_5Min, true)]       // SAP SE (Nasdaq)
	[TestCase("SAP", Models.AlphaVantage.EInterval.Interval_15Min, true)]       // SAP SE (Nasdaq)
	[TestCase("SAP", Models.AlphaVantage.EInterval.Interval_30Min, true)]       // SAP SE (Nasdaq)
	[TestCase("SAP", Models.AlphaVantage.EInterval.Interval_60Min, true)]       // SAP SE (Nasdaq)
	[TestCase("TESTING.NET", Models.AlphaVantage.EInterval.Interval_15Min, false)]
	public async Task GetHistoryIntradayRecordsAsync_ValidSymbols_ReturnsRecords(string symbol, Models.AlphaVantage.EInterval eInterval, bool shouldHave)
	{
		var startDay = new DateTime(2024, 12, 02);
		var endDay = new DateTime(2024, 12, 02);
		if (shouldHave)
		{
			var records = await _service.GetHistoryIntradayRecordsAsync(symbol, startDay, endDay, eInterval);
			Assert.That(records, Is.Not.Empty);
		}
		else
		{
			Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetHistoryIntradayRecordsAsync(symbol, startDay, endDay, eInterval));
		}
	}

	[TestCase("EUR", "USD", true)]
	[TestCase("TESTING1.NET", "TESTING2.NET", false)]
	public async Task GetHistoryForexRecordsAsync_ValidCurrencies_ReturnsRecords(string currency1, string currency2, bool shouldHave)
	{
		if (shouldHave)
		{
			var records = await _service.GetHistoryForexRecordsAsync(currency1, currency2, DateTime.UtcNow.AddDays(-3));
			Assert.That(records, Is.Not.Empty);
		}
		else
		{
			Assert.ThrowsAsync<FinanceNetException>(async () => await _service.GetHistoryForexRecordsAsync(currency1, currency2, DateTime.UtcNow.AddDays(-3)));
		}
	}
}

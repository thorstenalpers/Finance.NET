using System;
using System.Threading.Tasks;
using Finance.Net.Interfaces;
using Finance.Net.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Finance.Net.Tests.IntegrationTests;

[TestFixture]
[Category("IntegrationTests")]
public class AlphaVantageTests
{
	private IServiceProvider _serviceProvider;
	private IAlphaVantageService _service;

	[SetUp]
	public void SetUp()
	{
		_serviceProvider = TestHelper.SetUpServiceProvider();
		_service = _serviceProvider.GetRequiredService<IAlphaVantageService>();
	}

	[Test]
	public async Task GetCompanyOverviewAsync_WithoutIoC_ValidSymbols_ReturnsOverview()
	{
		var cfg = _serviceProvider.GetRequiredService<IOptions<FinanceNetConfiguration>>();
		var configuration = _serviceProvider.GetService<IConfiguration>();

		var service = AlphaVantageService.Create(new FinanceNetConfiguration
		{
			AlphaVantageApiKey = cfg.Value.AlphaVantageApiKey
		});

		var overview = await service.GetCompanyInfoAsync("SAP");

		Assert.That(overview, Is.Not.Null);
		Assert.That(overview.Symbol, Is.EqualTo("SAP"));
	}

	[TestCase("MSFT")]      // Microsoft Corporation (Nasdaq)
	[TestCase("SAP")]       // SAP SE (Nasdaq)
	[TestCase("GOOG")]      // Alphabet (Nasdaq)
	public async Task GetCompanyOverviewAsync_ValidSymbols_ReturnsOverview(string symbol)
	{

		var overview = await _service.GetCompanyInfoAsync(symbol);

		Assert.That(overview, Is.Not.Null);
		Assert.That(overview.Symbol, Is.EqualTo(symbol));
	}

	[TestCase("MSFT")]      // Microsoft Corporation (Nasdaq)
	[TestCase("SAP")]       // SAP SE (Nasdaq)
	[TestCase("SAP.DE")]    // SAP SE (Xetra)
	[TestCase("VOO")]       // Vanguard S&P 500 ETF
	public async Task GetDailyRecordsAsync_ValidSymbols_ReturnsRecords(string symbol)
	{
		var records = await _service.GetDailyRecordsAsync(symbol, DateTime.UtcNow.AddDays(-7));

		Assert.That(records, Is.Not.Empty);
	}

	[TestCase("MSFT")]      // Microsoft Corporation (Nasdaq)
	[TestCase("SAP")]       // SAP SE (Nasdaq)
	public async Task GetIntradayRecordsAsync_ValidSymbols_ReturnsRecords(string symbol)
	{
		var startDay = new DateTime(2024, 12, 02);
		var endDay = new DateTime(2024, 12, 02);
		var records = await _service.GetIntradayRecordsAsync(symbol, startDay, endDay);

		Assert.That(records, Is.Not.Empty);
	}

	[TestCase("EUR", "USD")]
	public async Task GetDailyForexRecordsAsync_ValidCurrencies_ReturnsRecords(string currency1, string currency2)
	{
		var records = await _service.GetDailyForexRecordsAsync(currency1, currency2, DateTime.UtcNow.AddDays(-3));

		Assert.That(records, Is.Not.Empty);
	}
}

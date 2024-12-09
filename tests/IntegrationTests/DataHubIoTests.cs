using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFinance.Interfaces;
using NetFinance.Services;
using NUnit.Framework;

namespace NetFinance.Tests.IntegrationTests;

[TestFixture]
[Category("IntegrationTests")]
public class DataHubIoTests
{
	private static IServiceProvider _serviceProvider;
	private IDatahubIoService _service;

	[SetUp]
	public void SetUp()
	{
		_serviceProvider = TestHelper.SetUpServiceProvider();
		_service = _serviceProvider.GetRequiredService<IDatahubIoService>();
	}

	[Test]
	public async Task GetNasdaqInstrumentsAsync_WithoutIoC_ReturnsInstruments()
	{
		var service = DatahubIoService.Create();
		var instruments = await service.GetNasdaqInstrumentsAsync();

		Assert.That(instruments, Is.Not.Empty);
	}

	[TestCase("MSFT")]      // Microsoft Corporation (Nasdaq)
	[TestCase("GOOG")]      // Alphabet (Nasdaq)
	public async Task GetNasdaqInstrumentsAsync_ValidSymbols_ReturnsInstruments(string symbol)
	{
		var instruments = await _service.GetNasdaqInstrumentsAsync();

		Assert.That(instruments, Is.Not.Empty);

		var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);

		Assert.That(instrument, Is.Not.Null);
		Assert.That(instrument?.SecurityName, Is.Not.Empty);
	}

	[TestCase("MSFT")]      // Microsoft Corporation (Nasdaq)
	[TestCase("GOOG")]      // Alphabet (Nasdaq)
	public async Task GetSAndP500InstrumentsAsync_ValidSymbols_ReturnsInstruments(string symbol)
	{
		var instruments = await _service.GetSAndP500InstrumentsAsync();

		Assert.That(instruments, Is.Not.Empty);

		var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);

		Assert.That(instrument, Is.Not.Null);
		Assert.That(instrument?.Name, Is.Not.Empty);
	}

}

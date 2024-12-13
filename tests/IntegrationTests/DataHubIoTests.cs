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
public class DatahubIoTests
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
    public async Task GetSP500InstrumentsAsync_ValidSymbols_ReturnsInstruments(string symbol)
    {
        var instruments = await _service.GetSP500InstrumentsAsync();

        Assert.That(instruments, Is.Not.Empty);

        var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);

        Assert.That(instrument, Is.Not.Null);
        Assert.That(instrument?.Name, Is.Not.Empty);
    }
}

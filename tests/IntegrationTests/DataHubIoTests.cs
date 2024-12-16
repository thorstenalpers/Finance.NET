using System;
using System.Linq;
using System.Threading.Tasks;
using Finance.Net.Interfaces;
using Finance.Net.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Finance.Net.Tests.IntegrationTests;

[TestFixture]
[Category("Integration")]
public class DatahubIoTests
{
    private static IServiceProvider s_serviceProvider;
    private IDatahubIoService _service;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        s_serviceProvider = TestHelper.SetUpServiceProvider();
        _service = s_serviceProvider.GetRequiredService<IDatahubIoService>();
    }

    [Test]
    public async Task GetNasdaqInstrumentsAsync_StaticInstance_ReturnsInstruments()
    {
        var service = DatahubIoService.Create();
        var instruments = await service.GetNasdaqInstrumentsAsync();

        Assert.That(instruments, Is.Not.Empty);
    }

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("GOOG", true)]      // Alphabet (Nasdaq)
    [TestCase("TESTING.NET", false)]
    public async Task GetNasdaqInstrumentsAsync_ValidSymbols_ReturnsInstruments(string symbol, bool shouldHave)
    {
        var instruments = await _service.GetNasdaqInstrumentsAsync();
        var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);
        if (shouldHave)
        {
            Assert.That(instrument, Is.Not.Null);
            Assert.That(instrument?.SecurityName, Is.Not.Empty);
        }
        else
        {
            Assert.That(instrument, Is.Null);
            Assert.That(instrument?.SecurityName, Is.Null.Or.Empty);
        }
    }

    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("GOOG", true)]      // Alphabet (Nasdaq)
    [TestCase("TESTING.NET", false)]
    public async Task GetSP500InstrumentsAsync_ValidSymbols_ReturnsInstruments(string symbol, bool shouldHave)
    {
        var instruments = await _service.GetSP500InstrumentsAsync();
        var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);
        if (shouldHave)
        {
            Assert.That(instrument, Is.Not.Null);
            Assert.That(instrument?.Name, Is.Not.Empty);
        }
        else
        {
            Assert.That(instrument, Is.Null);
            Assert.That(instrument?.Name, Is.Null.Or.Empty);
        }
    }
}

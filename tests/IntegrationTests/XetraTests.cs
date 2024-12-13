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
public class XetraTests
{
    private static IServiceProvider s_serviceProvider;
    private IXetraService _service;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        s_serviceProvider = TestHelper.SetUpServiceProvider();
        _service = s_serviceProvider.GetRequiredService<IXetraService>();
    }

    [TestCase("MSF.DE")]    // Microsoft Corporation (Xetra)
    [TestCase("SAP.DE")]    // SAP SE (Xetra)
    [TestCase("VUSA.DE")]   // Vanguard S&P 500 ETF
    public async Task GetInstruments_ValidSymbols_ReturnsIntsruments(string symbol)
    {
        var instruments = await _service.GetInstruments();

        Assert.That(instruments, Is.Not.Empty);

        var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);

        Assert.That(instrument, Is.Not.Null);
        Assert.That(instrument?.ISIN, Is.Not.Empty);
        Assert.That(instrument?.InstrumentName, Is.Not.Empty);
    }

    [Test]
    public async Task GetTradableInstruments_WithoutIoC_ReturnsInstruments()
    {
        var service = XetraService.Create();
        var instruments = await service.GetInstruments();

        Assert.That(instruments, Is.Not.Empty);
    }
}

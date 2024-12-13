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

    [TestCase("MSF.DE", true)]    // Microsoft Corporation (Xetra)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("VUSA.DE", true)]   // Vanguard S&P 500 ETF
    [TestCase("TESTING.NET", false)]
    public async Task GetInstruments_ValidSymbols_ReturnsIntsruments(string symbol, bool shouldHave)
    {
        var instruments = await _service.GetInstruments();
        var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);

        if (shouldHave)
        {
            Assert.That(instrument, Is.Not.Null);
            Assert.That(instrument?.ISIN, Is.Not.Empty);
            Assert.That(instrument?.InstrumentName, Is.Not.Empty);
        }
        else
        {
            Assert.That(instrument, Is.Null);
            Assert.That(instrument?.ISIN, Is.Null.Or.Empty);
            Assert.That(instrument?.InstrumentName, Is.Null.Or.Empty);
        }
    }

    [Test]
    public async Task GetTradableInstruments_WithoutIoC_ReturnsInstruments()
    {
        var service = XetraService.Create();
        var instruments = await service.GetInstruments();

        Assert.That(instruments, Is.Not.Empty);
    }
}

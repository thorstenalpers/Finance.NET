using System.Linq;
using System.Threading.Tasks;
using Finance.Net.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Finance.Net.Tests.IntegrationTests;

[TestFixture]
[Category("Integration")]
public class XetraTests
{
    private IXetraService _service;

    [SetUp]
    public void SetUp()
    {
        var serviceProvider = TestHelper.SetUpServiceProvider();
        _service = serviceProvider.GetRequiredService<IXetraService>();
    }

    [TestCase("TL0.DE", true)]    // Tesla (Xetra)
    [TestCase("MSF.DE", true)]    // Microsoft Corporation (Xetra)
    [TestCase("SAP.DE", true)]    // SAP SE (Xetra)
    [TestCase("VUSA.DE", true)]   // Vanguard S&P 500 ETF
    [TestCase("TESTING.NET", false)]
    public async Task GetInstrumentsAsync(string symbol, bool shouldHave)
    {
        var instruments = await _service.GetInstrumentsAsync();
        var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);

        if (shouldHave)
        {
            Assert.That(instrument, Is.Not.Null);
            Assert.That(instrument?.ISIN, Is.Not.Empty);
            Assert.That(instrument?.InstrumentName, Is.Not.Empty);
            Assert.Pass(JsonConvert.SerializeObject(instrument));
        }
        else
        {
            Assert.That(instrument, Is.Null);
            Assert.That(instrument?.ISIN, Is.Null.Or.Empty);
            Assert.That(instrument?.InstrumentName, Is.Null.Or.Empty);
        }
    }

    [TestCase("TL0.DE")]    // Tesla (Xetra)
    [TestCase("MSF.DE")]    // Microsoft Corporation (Xetra)
    [TestCase("SAP.DE")]    // SAP SE (Xetra)
    [TestCase("VUSA.DE")]   // Vanguard S&P 500 ETF
    [TestCase("MSF.DE")]       // Microsoft
    [TestCase("IBM.DE")]       // IBM
    [TestCase("TL0.DE")]       // Tesla
    public async Task GetInstrumentsAsync(string symbol)
    {
        var instruments = await _service.GetInstrumentsAsync();
        var instrument = instruments.FirstOrDefault(e => e.Symbol == symbol);

        Assert.That(instrument, Is.Not.Null);
        Assert.That(instrument?.ISIN, Is.Not.Empty);
        Assert.That(instrument?.InstrumentName, Is.Not.Empty);
        Assert.Pass(JsonConvert.SerializeObject(instrument));
    }
}

using System.Linq;
using System.Threading.Tasks;
using Finance.Net.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Finance.Net.Tests.IntegrationTests;

[TestFixture]
[Category("Integration")]
public class DataHubTests
{
    private IDataHubService _service;

    [SetUp]
    public void SetUp()
    {
        var serviceProvider = TestHelper.SetUpServiceProvider();
        _service = serviceProvider.GetRequiredService<IDataHubService>();
    }

    [TestCase("TSLA", true)]      // Tesla (Stock - Nasdaq)
    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("GOOG", true)]      // Alphabet (Nasdaq)
    [TestCase("TESTING.NET", false)]
    public async Task GetNasdaqInstrumentsAsync(string symbol, bool shouldHave)
    {
        var tickers = await _service.GetNasdaqInstrumentsAsync();
        var ticker = tickers.FirstOrDefault(e => e.Symbol == symbol);
        if (shouldHave)
        {
            Assert.That(ticker, Is.Not.Null);
            Assert.That(ticker?.Name, Is.Not.Empty);
            Assert.That(ticker?.Symbol, Is.Not.Empty);
            Assert.Pass(JsonConvert.SerializeObject(ticker));
        }
        else
        {
            Assert.That(ticker, Is.Null);
            Assert.That(ticker?.Name, Is.Null.Or.Empty);
            Assert.That(ticker?.Symbol, Is.Null.Or.Empty);
        }
    }

    [TestCase("TSLA", true)]      // Tesla (Stock - Nasdaq)
    [TestCase("MSFT", true)]      // Microsoft Corporation (Nasdaq)
    [TestCase("GOOG", true)]      // Alphabet (Nasdaq)
    [TestCase("TESTING.NET", false)]
    public async Task GetSp500InstrumentsAsync(string symbol, bool shouldHave)
    {
        var tickers = await _service.GetSp500InstrumentsAsync();
        var ticker = tickers.FirstOrDefault(e => e.Symbol == symbol);
        if (shouldHave)
        {
            Assert.That(ticker, Is.Not.Null);
            Assert.That(ticker?.Name, Is.Not.Empty);
            Assert.Pass(JsonConvert.SerializeObject(ticker));
        }
        else
        {
            Assert.That(ticker, Is.Null);
            Assert.That(ticker?.Name, Is.Null.Or.Empty);
        }
    }
}

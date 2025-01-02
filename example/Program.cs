using System.Reflection;
using Finance.Net;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Extensions;
using Finance.Net.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Example;

/// <summary>
/// This example demonstrates searching for a ticker name in a dataset, retrieving matching records, 
/// and printing the details to the console.
/// </summary>
public class Program
{
    static async Task Main()
    {
        // search for a ticker 
        var tickerName = "Tesla";

        // initialization
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() },
            Formatting = Formatting.Indented
        };
        var services = new ServiceCollection();
        var cfg = new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly()).Build();
        var alphaVantageApiKey = cfg["FinanceNet:AlphaVantageApiKey"];      // paste your API key here
        services.AddFinanceNet(new FinanceNetConfiguration
        {
            AlphaVantageApiKey = alphaVantageApiKey
        });
        var provider = services.BuildServiceProvider();
        var yahooService = provider.GetRequiredService<IYahooFinanceService>();
        var alphaService = provider.GetRequiredService<IAlphaVantageService>();
        var datahubService = provider.GetRequiredService<IDatahubService>();
        var xetraService = provider.GetRequiredService<IXetraService>();

        // get instruments from different providers
        var symbolInfosXetra = await xetraService.GetInstrumentsAsync();
        Console.WriteLine($"\n---\nInstruments from Xetra\n---");
        Console.WriteLine("\n" + JsonConvert.SerializeObject(symbolInfosXetra.FirstOrDefault(e => e.InstrumentName?.Contains(tickerName, StringComparison.InvariantCultureIgnoreCase) ?? false)));

        var sp500Instruments = await datahubService.GetSp500InstrumentsAsync();
        Console.WriteLine($"\n---\nInstruments from Datahub S&P500\n---");
        Console.WriteLine("\n" + JsonConvert.SerializeObject(sp500Instruments.FirstOrDefault(e => e.Name?.Contains(tickerName, StringComparison.InvariantCultureIgnoreCase) ?? false)));

        var nasdaqInstruments = await datahubService.GetNasdaqInstrumentsAsync();
        var instrument = nasdaqInstruments.FirstOrDefault(e => e.Name?.Contains(tickerName, StringComparison.InvariantCultureIgnoreCase) ?? false);
        Console.WriteLine($"\n---\nInstruments from Datahub Nasdaq\n---");
        Console.WriteLine("\n" + JsonConvert.SerializeObject(instrument));

        var yahooInstruments = await yahooService.GetInstrumentsAsync(EAssetType.Stock);
        Console.WriteLine($"\n---\nInstruments from Yahoo Finance\n---");
        Console.WriteLine("\n" + JsonConvert.SerializeObject(yahooInstruments.FirstOrDefault(e => e.Symbol == instrument?.Symbol)));

        if (instrument?.Symbol == null)
        {
            throw new FinanceNetException("instrument?.Symbol is null");
        }

        // get fundamental data
        var summary = await yahooService.GetSummaryAsync(instrument.Symbol);
        var profile = await yahooService.GetProfileAsync(instrument.Symbol);
        var financials = await yahooService.GetFinancialsAsync(instrument.Symbol);
        Console.WriteLine($"\n---\nFundamentals from Yahoo\n---");
        Console.WriteLine("\n" + JsonConvert.SerializeObject(summary));
        Console.WriteLine("\n" + JsonConvert.SerializeObject(profile));
        Console.WriteLine("\n" + JsonConvert.SerializeObject(financials?.FirstOrDefault()));
        Console.WriteLine($"Name={summary?.Name}, Industry={profile?.Industry}, Sector={profile?.Sector}");

        // get records
        var recordsYahoo = await yahooService.GetRecordsAsync(instrument.Symbol);
        Console.WriteLine($"\n---\nRecords from Yahoo\n---");
        foreach (var record in recordsYahoo)
        {
            Console.WriteLine($"{record.Date:yyyy-MM-dd}: {record.Open?.ToString("F2")} / {record.AdjustedClose?.ToString("F2")} (Open / Close)");
        }

        // from alpha vantage if API key present
        if (!string.IsNullOrWhiteSpace(alphaVantageApiKey))
        {
            // get company overview
            var overview = await alphaService.GetOverviewAsync(instrument.Symbol);
            Console.WriteLine($"\n---\nCompany Overview from Alpha Vantage\n---");
            Console.WriteLine("\n" + JsonConvert.SerializeObject(overview));

            // get records
            var recordsAlpha = await alphaService.GetRecordsAsync(instrument.Symbol, startDate: new DateTime(2024, 01, 01), endDate: new DateTime(2024, 01, 31));
            Console.WriteLine($"\n---\nRecords from Alpha Vantage\n---");
            foreach (var record in recordsAlpha)
            {
                Console.WriteLine($"{record.Date:yyyy-MM-dd}: {record.Open?.ToString("F2")} / {record.AdjustedClose?.ToString("F2")} (Open / Close)");
            }
        }

        // get real time data
        var quote = await yahooService.GetQuoteAsync(instrument.Symbol);
        Console.WriteLine($"\n---\nReal-Time Quote from Yahoo\n---");
        Console.WriteLine("\n" + JsonConvert.SerializeObject(quote));
        Console.WriteLine($"Time={quote.RegularMarketTime}, " +
            $"Price={quote.RegularMarketPrice}, " +
            $"Bid={quote.Bid}, " +
            $"Ask={quote.Ask}");
    }
}

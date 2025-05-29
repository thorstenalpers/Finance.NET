﻿using CsvHelper.Configuration;
using Finance.Net.Models.DataHub;

namespace Finance.Net.Mappings;

internal class SP500InstrumentMapping : ClassMap<Sp500Instrument>
{
    public SP500InstrumentMapping()
    {
        InitializeMappings();
    }

    private void InitializeMappings()
    {
        Map(m => m.Symbol).Name("Symbol");
        Map(m => m.Name).Name("Name");
        Map(m => m.Sector).Name("Sector");
        Map(m => m.Price).Name("Price");
        Map(m => m.PriceEarnings).Name("Price/Earnings");
        Map(m => m.DividendYield).Name("Dividend Yield");
        Map(m => m.EarningsShare).Name("Earnings/Share");
        Map(m => m.FiftyTwoWeekLow).Name("52 Week Low");
        Map(m => m.FiftyTwoWeekHigh).Name("52 Week High");
        Map(m => m.MarketCap).Name("Market Cap");
        Map(m => m.EBITDA).Name("EBITDA");
        Map(m => m.PriceSales).Name("Price/Sales");
        Map(m => m.PriceBook).Name("Price/Book");
    }
}

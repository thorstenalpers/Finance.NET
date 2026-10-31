using CsvHelper;
using Finance.Net.Models.DataHub;

namespace Finance.Net.Mappings;

[CsvRecordMap(typeof(Sp500Instrument))]
internal partial class SP500InstrumentMapping
{
    [CsvColumn("Symbol")]
    public string? Symbol { get; set; }

    [CsvColumn("Name")]
    public string? Name { get; set; }

    [CsvColumn("Sector")]
    public string? Sector { get; set; }

    [CsvColumn("Price")]
    public double? Price { get; set; }

    [CsvColumn("Price/Earnings")]
    public double? PriceEarnings { get; set; }

    [CsvColumn("Dividend Yield")]
    public double? DividendYield { get; set; }

    [CsvColumn("Earnings/Share")]
    public double? EarningsShare { get; set; }

    [CsvColumn("52 Week Low")]
    public double? FiftyTwoWeekLow { get; set; }

    [CsvColumn("52 Week High")]
    public double? FiftyTwoWeekHigh { get; set; }

    [CsvColumn("Market Cap")]
    public long? MarketCap { get; set; }

    [CsvColumn("EBITDA")]
    public long? EBITDA { get; set; }

    [CsvColumn("Price/Sales")]
    public double? PriceSales { get; set; }

    [CsvColumn("Price/Book")]
    public double? PriceBook { get; set; }
}

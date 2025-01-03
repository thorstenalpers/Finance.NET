namespace Finance.Net.Models.DataHub;

/// <summary>
/// Represents a financial instrument in the S&amp;P 500 index.
/// </summary>
public class Sp500Instrument
{
    /// <summary>
    /// The ticker symbol of the instrument in the S&amp;P 500 index.
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// The name of the instrument or the company it represents.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The sector to which the instrument belongs.
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// The current price of the instrument.
    /// </summary>
    public double? Price { get; set; }

    /// <summary>
    /// The price-to-earnings ratio of the instrument.
    /// </summary>
    public double? PriceEarnings { get; set; }

    /// <summary>
    /// The dividend yield of the instrument.
    /// </summary>
    public double? DividendYield { get; set; }

    /// <summary>
    /// The earnings per share of the instrument.
    /// </summary>
    public double? EarningsShare { get; set; }

    /// <summary>
    /// The 52-week low price of the instrument.
    /// </summary>
    public double? FiftyTwoWeekLow { get; set; }

    /// <summary>
    /// The 52-week high price of the instrument.
    /// </summary>
    public double? FiftyTwoWeekHigh { get; set; }

    /// <summary>
    /// The market capitalization of the instrument.
    /// </summary>
    public long? MarketCap { get; set; }

    /// <summary>
    /// The EBITDA (Earnings Before Interest, Taxes, Depreciation, and Amortization) of the instrument.
    /// </summary>
    public long? EBITDA { get; set; }

    /// <summary>
    /// The price-to-sales ratio of the instrument.
    /// </summary>
    public double? PriceSales { get; set; }

    /// <summary>
    /// The price-to-book ratio of the instrument.
    /// </summary>
    public double? PriceBook { get; set; }
}

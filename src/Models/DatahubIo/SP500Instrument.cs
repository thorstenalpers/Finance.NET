namespace Finance.Net.Models.DatahubIo;

/// <summary>
/// Represents a financial instrument in the S&amp;P 500 index, including details such as price, earnings, dividend yield, and other financial metrics.
/// </summary>
public class SP500Instrument
{
    /// <summary>
    /// The symbol of the instrument in the S&amp;P 500 index.
    /// </summary>
    /// <value>The symbol of the instrument, or <c>null</c> if not available.</value>
    public string? Symbol { get; set; }

    /// <summary>
    /// The name of the instrument or the company it represents.
    /// </summary>
    /// <value>The name of the instrument, or <c>null</c> if not available.</value>
    public string? Name { get; set; }

    /// <summary>
    /// The sector to which the instrument belongs.
    /// </summary>
    /// <value>The sector, or <c>null</c> if not available.</value>
    public string? Sector { get; set; }

    /// <summary>
    /// The current price of the instrument.
    /// </summary>
    /// <value>The price of the instrument, or <c>null</c> if not available.</value>
    public double? Price { get; set; }

    /// <summary>
    /// The price-to-earnings ratio of the instrument.
    /// </summary>
    /// <value>The price-to-earnings ratio, or <c>null</c> if not available.</value>
    public double? PriceEarnings { get; set; }

    /// <summary>
    /// The dividend yield of the instrument.
    /// </summary>
    /// <value>The dividend yield, or <c>null</c> if not available.</value>
    public double? DividendYield { get; set; }

    /// <summary>
    /// The earnings per share of the instrument.
    /// </summary>
    /// <value>The earnings per share, or <c>null</c> if not available.</value>
    public double? EarningsShare { get; set; }

    /// <summary>
    /// The 52-week low price of the instrument.
    /// </summary>
    /// <value>The 52-week low price, or <c>null</c> if not available.</value>
    public double? num52WeekLow { get; set; }

    /// <summary>
    /// The 52-week high price of the instrument.
    /// </summary>
    /// <value>The 52-week high price, or <c>null</c> if not available.</value>
    public double? num52WeekHigh { get; set; }

    /// <summary>
    /// The market capitalization of the instrument.
    /// </summary>
    /// <value>The market capitalization, or <c>null</c> if not available.</value>
    public long? MarketCap { get; set; }

    /// <summary>
    /// The EBITDA (Earnings Before Interest, Taxes, Depreciation, and Amortization) of the instrument.
    /// </summary>
    /// <value>The EBITDA, or <c>null</c> if not available.</value>
    public long? EBITDA { get; set; }

    /// <summary>
    /// The price-to-sales ratio of the instrument.
    /// </summary>
    /// <value>The price-to-sales ratio, or <c>null</c> if not available.</value>
    public double? PriceSales { get; set; }

    /// <summary>
    /// The price-to-book ratio of the instrument.
    /// </summary>
    /// <value>The price-to-book ratio, or <c>null</c> if not available.</value>
    public double? PriceBook { get; set; }

    /// <summary>
    /// The SEC filings associated with the instrument.
    /// </summary>
    /// <value>The SEC filings, or <c>null</c> if not available.</value>
    public string? SECFilings { get; set; }
}

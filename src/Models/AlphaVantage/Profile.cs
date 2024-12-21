using Newtonsoft.Json;

namespace Finance.Net.Models.AlphaVantage;

/// <summary>
/// Represents an overview of a company, including financial and descriptive information.
/// </summary>
public class Profile
{
    /// <summary>
    /// The stock symbol of the company.
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// The type of asset the company represents.
    /// </summary>
    public string? AssetType { get; set; }

    /// <summary>
    /// The name of the company.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// A brief description of the company.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The Central Index Key (CIK) of the company.
    /// </summary>
    public string? CIK { get; set; }

    /// <summary>
    /// The exchange on which the company is listed.
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// The currency used for the company's financials.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// The country in which the company is located.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// The sector to which the company belongs.
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// The industry in which the company operates.
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// The address of the company's headquarters.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// The official website of the company.
    /// </summary>
    public string? OfficialSite { get; set; }

    /// <summary>
    /// The fiscal year end of the company.
    /// </summary>
    public string? FiscalYearEnd { get; set; }

    /// <summary>
    /// The latest quarter for which data is available.
    /// </summary>
    public string? LatestQuarter { get; set; }

    /// <summary>
    /// The market capitalization of the company.
    /// </summary>
    public long? MarketCapitalization { get; set; }

    /// <summary>
    /// Earnings Before Interest, Taxes, Depreciation, and Amortization (EBITDA) of the company.
    /// </summary>
    public string? EBITDA { get; set; }

    /// <summary>
    /// The Price-to-Earnings (P/E) ratio of the company.
    /// </summary>
    public string? PERatio { get; set; }

    /// <summary>
    /// The Price/Earnings-to-Growth (PEG) ratio of the company.
    /// </summary>
    public string? PEGRatio { get; set; }

    /// <summary>
    /// The book value of the company.
    /// </summary>
    public string? BookValue { get; set; }

    /// <summary>
    /// The dividend per share paid by the company.
    /// </summary>
    public string? DividendPerShare { get; set; }

    /// <summary>
    /// The dividend yield of the company.
    /// </summary>
    public string? DividendYield { get; set; }

    /// <summary>
    /// Earnings per share (EPS) of the company.
    /// </summary>
    public string? EPS { get; set; }

    /// <summary>
    /// The revenue per share for the trailing twelve months (TTM).
    /// </summary>
    public string? RevenuePerShareTTM { get; set; }

    /// <summary>
    /// The profit margin of the company.
    /// </summary>
    public string? ProfitMargin { get; set; }

    /// <summary>
    /// The operating margin for the trailing twelve months (TTM).
    /// </summary>
    public string? OperatingMarginTTM { get; set; }

    /// <summary>
    /// The return on assets (ROA) for the trailing twelve months (TTM).
    /// </summary>
    public string? ReturnOnAssetsTTM { get; set; }

    /// <summary>
    /// The return on equity (ROE) for the trailing twelve months (TTM).
    /// </summary>
    public string? ReturnOnEquityTTM { get; set; }

    /// <summary>
    /// The revenue for the trailing twelve months (TTM).
    /// </summary>
    public string? RevenueTTM { get; set; }

    /// <summary>
    /// The gross profit for the trailing twelve months (TTM).
    /// </summary>
    public string? GrossProfitTTM { get; set; }

    /// <summary>
    /// The diluted earnings per share (EPS) for the trailing twelve months (TTM).
    /// </summary>
    public string? DilutedEPSTTM { get; set; }

    /// <summary>
    /// The quarterly earnings growth year-over-year (YOY).
    /// </summary>
    public string? QuarterlyEarningsGrowthYOY { get; set; }

    /// <summary>
    /// The quarterly revenue growth year-over-year (YOY).
    /// </summary>
    public string? QuarterlyRevenueGrowthYOY { get; set; }

    /// <summary>
    /// The analyst target price for the company's stock.
    /// </summary>
    public string? AnalystTargetPrice { get; set; }

    /// <summary>
    /// The percentage of analysts recommending a strong buy for the stock.
    /// </summary>
    public string? AnalystRatingStrongBuy { get; set; }

    /// <summary>
    /// The percentage of analysts recommending a buy for the stock.
    /// </summary>
    public string? AnalystRatingBuy { get; set; }

    /// <summary>
    /// The percentage of analysts recommending a hold for the stock.
    /// </summary>
    public string? AnalystRatingHold { get; set; }

    /// <summary>
    /// The percentage of analysts recommending a sell for the stock.
    /// </summary>
    public string? AnalystRatingSell { get; set; }

    /// <summary>
    /// The percentage of analysts recommending a strong sell for the stock.
    /// </summary>
    public string? AnalystRatingStrongSell { get; set; }

    /// <summary>
    /// The trailing Price-to-Earnings (P/E) ratio of the company.
    /// </summary>
    public string? TrailingPE { get; set; }

    /// <summary>
    /// The forward Price-to-Earnings (P/E) ratio of the company.
    /// </summary>
    public string? ForwardPE { get; set; }

    /// <summary>
    /// The Price-to-Sales ratio for the trailing twelve months (TTM).
    /// </summary>
    public string? PriceToSalesRatioTTM { get; set; }

    /// <summary>
    /// The Price-to-Book ratio of the company.
    /// </summary>
    public string? PriceToBookRatio { get; set; }

    /// <summary>
    /// The enterprise value-to-revenue ratio of the company.
    /// </summary>
    public string? EVToRevenue { get; set; }

    /// <summary>
    /// The enterprise value-to-EBITDA ratio of the company.
    /// </summary>
    public string? EVToEBITDA { get; set; }

    /// <summary>
    /// The beta value of the company's stock, which measures its volatility relative to the market.
    /// </summary>
    public string? Beta { get; set; }

    /// <summary>
    /// The 52-week high price of the company's stock.
    /// </summary>
    [JsonProperty("52WeekHigh")]
    public string? _52WeekHigh { get; set; }

    /// <summary>
    /// The 52-week low price of the company's stock.
    /// </summary>
    [JsonProperty("52WeekLow")]
    public string? _52WeekLow { get; set; }

    /// <summary>
    /// The 50-day moving average of the company's stock price.
    /// </summary>
    [JsonProperty("50DayMovingAverage")]
    public string? _50DayMovingAverage { get; set; }

    /// <summary>
    /// The 200-day moving average of the company's stock price.
    /// </summary>
    [JsonProperty("200DayMovingAverage")]
    public string? _200DayMovingAverage { get; set; }

    /// <summary>
    /// The number of shares outstanding for the company.
    /// </summary>
    public string? SharesOutstanding { get; set; }

    /// <summary>
    /// The date of the company's next dividend payment.
    /// </summary>
    public string? DividendDate { get; set; }

    /// <summary>
    /// The ex-dividend date for the company's stock.
    /// </summary>
    public string? ExDividendDate { get; set; }
}

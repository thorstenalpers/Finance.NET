using System;

namespace Finance.Net.Models.Yahoo;

/// <summary>
/// Represents a summary of key financial metrics and stock information for a company.
/// </summary>
public record Summary
{
    /// <summary>
    /// Name of asset.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// A notice indicating the market's open or close time.
    /// </summary>
    public string? MarketTimeNotice { get; set; }

    /// <summary>
    /// The previous closing price of the stock.
    /// </summary>
    public decimal? PreviousClose { get; set; }

    /// <summary>
    /// The opening price of the stock for the current trading session.
    /// </summary>
    public decimal? Open { get; set; }

    /// <summary>
    /// The current bid price for the stock.
    /// </summary>
    public decimal? Bid { get; set; }

    /// <summary>
    /// The current ask price for the stock.
    /// </summary>
    public decimal? Ask { get; set; }

    /// <summary>
    /// The minimum price for the stock during the current trading day.
    /// </summary>
    public decimal? DaysRange_Min { get; set; }

    /// <summary>
    /// The maximum price for the stock during the current trading day.
    /// </summary>
    public decimal? DaysRange_Max { get; set; }

    /// <summary>
    /// The minimum price for the stock over the past 52 weeks.
    /// </summary>
    public decimal? WeekRange52_Min { get; set; }

    /// <summary>
    /// The maximum price for the stock over the past 52 weeks.
    /// </summary>
    public decimal? WeekRange52_Max { get; set; }

    /// <summary>
    /// The total volume of the stock traded during the current session.
    /// </summary>
    public decimal? Volume { get; set; }

    /// <summary>
    /// The average daily volume of the stock traded over a specified period.
    /// </summary>
    public decimal? AvgVolume { get; set; }

    /// <summary>
    /// The market capitalization during the current trading session.
    /// </summary>
    public decimal? MarketCap_Intraday { get; set; }

    /// <summary>
    /// The 5-year beta of the stock, calculated using monthly data, which measures the stock's volatility relative to the market.
    /// </summary>
    public decimal? Beta_5Y_Monthly { get; set; }

    /// <summary>
    /// The price-to-earnings (P/E) ratio for the stock, calculated using trailing twelve months (TTM) data.
    /// </summary>
    public decimal? PE_Ratio_TTM { get; set; }

    /// <summary>
    /// The earnings per share (EPS) for the stock, calculated using trailing twelve months (TTM) data.
    /// </summary>
    public decimal? EPS_TTM { get; set; }

    /// <summary>
    /// The date for the company's upcoming earnings report.
    /// </summary>
    public DateTime? EarningsDate { get; set; }

    /// <summary>
    /// The forward dividend expected to be paid per share.
    /// </summary>
    public decimal? Forward_Dividend { get; set; }

    /// <summary>
    /// The forward dividend yield, calculated as the forward dividend divided by the stock price.
    /// </summary>
    public decimal? Forward_Yield { get; set; }

    /// <summary>
    /// The ex-dividend date, which is the date after which new buyers of the stock are not entitled to the upcoming dividend.
    /// </summary>
    public DateTime? Ex_DividendDate { get; set; }

    /// <summary>
    /// The one-year target estimate for the stock price, based on analysts' predictions.
    /// </summary>
    public decimal? OneYearTargetEst { get; set; }
}

using System;

namespace Finance.Net.Models.Yahoo;

/// <summary>
/// Represents the financial quote data of a stock, including various stock metrics, trading information, and other related details.
/// </summary>
public record Quote
{
    /// <summary>
    /// The language of the quote.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The region of the quote.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// The type of quote.
    /// </summary>
    public string? QuoteType { get; set; }

    /// <summary>
    /// The display type of the quote.
    /// </summary>
    public string? TypeDisp { get; set; }

    /// <summary>
    /// The source of the quote.
    /// </summary>
    public string? QuoteSourceName { get; set; }

    /// <summary>
    /// Indicates whether the quote is triggerable for price alerts.
    /// </summary>
    public bool? Triggerable { get; set; }

    /// <summary>
    /// The confidence level of a custom price alert.
    /// </summary>
    public string? CustomPriceAlertConfidence { get; set; }

    /// <summary>
    /// The currency in which the stock is traded.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// The exchange on which the stock is listed.
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// The short name of the quoted instrument.
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// The full name of the quoted instrument.
    /// </summary>
    public string? LongName { get; set; }

    /// <summary>
    /// The message board ID associated with the quoted instrument.
    /// </summary>
    public string? MessageBoardId { get; set; }

    /// <summary>
    /// The time zone of the exchange for the quoted instrument.
    /// </summary>
    public string? ExchangeTimezoneName { get; set; }

    /// <summary>
    /// The abbreviated time zone of the exchange for the quoted instrument.
    /// </summary>
    public string? ExchangeTimezoneShortName { get; set; }

    /// <summary>
    /// The GMT offset in milliseconds for the exchange's time zone.
    /// </summary>
    public long? GmtOffSetMilliseconds { get; set; }

    /// <summary>
    /// The market the instrument is listed on.
    /// </summary>
    public string? Market { get; set; }

    /// <summary>
    /// Indicates if ESG (Environmental, Social, Governance) data is available for the quote.
    /// </summary>
    public bool? EsgPopulated { get; set; }

    /// <summary>
    /// The percentage change in the regular market price.
    /// </summary>
    public double? RegularMarketChangePercent { get; set; }

    /// <summary>
    /// The regular market price of the quoted instrument.
    /// </summary>
    public double? RegularMarketPrice { get; set; }

    /// <summary>
    /// The market state (e.g., open or closed).
    /// </summary>
    public string? MarketState { get; set; }

    /// <summary>
    /// The full name of the exchange for the quoted instrument.
    /// </summary>
    public string? FullExchangeName { get; set; }

    /// <summary>
    /// The financial currency used for the quote.
    /// </summary>
    public string? FinancialCurrency { get; set; }

    /// <summary>
    /// The opening price of the quoted instrument in the regular market.
    /// </summary>
    public double? RegularMarketOpen { get; set; }

    /// <summary>
    /// The average volume over the last 3 months.
    /// </summary>
    public long? AverageDailyVolume3Month { get; set; }

    /// <summary>
    /// The average volume over the last 10 days.
    /// </summary>
    public long? AverageDailyVolume10Day { get; set; }

    /// <summary>
    /// The change in the 52-week low price of the quoted instrument.
    /// </summary>
    public double? FiftyTwoWeekLowChange { get; set; }

    /// <summary>
    /// The percentage change in the 52-week low price of the quoted instrument.
    /// </summary>
    public double? FiftyTwoWeekLowChangePercent { get; set; }

    /// <summary>
    /// The 52-week price range of the quoted instrument.
    /// </summary>
    public string? FiftyTwoWeekRange { get; set; }

    /// <summary>
    /// The change in the 52-week high price of the quoted instrument.
    /// </summary>
    public double? FiftyTwoWeekHighChange { get; set; }

    /// <summary>
    /// The percentage change in the 52-week high price of the quoted instrument.
    /// </summary>
    public double? FiftyTwoWeekHighChangePercent { get; set; }

    /// <summary>
    /// The price of the quoted instrument at its 52-week low.
    /// </summary>
    public double? FiftyTwoWeekLow { get; set; }

    /// <summary>
    /// The price of the quoted instrument at its 52-week high.
    /// </summary>
    public double? FiftyTwoWeekHigh { get; set; }

    /// <summary>
    /// The percentage change in the 52-week price of the quoted instrument.
    /// </summary>
    public double? FiftyTwoWeekChangePercent { get; set; }

    /// <summary>
    /// The earnings date for the quoted instrument.
    /// </summary>
    public DateTime? EarningsDate { get; set; }

    /// <summary>
    /// The start date for the earnings period.
    /// </summary>
    public DateTime? EarningsDateStart { get; set; }

    /// <summary>
    /// The end date for the earnings period.
    /// </summary>
    public DateTime? EarningsDateEnd { get; set; }

    /// <summary>
    /// The start date for the earnings call period.
    /// </summary>
    public DateTime? EarningsCallDateStart { get; set; }

    /// <summary>
    /// The end date for the earnings call period.
    /// </summary>
    public DateTime? EarningsCallDateEnd { get; set; }

    /// <summary>
    /// Indicates if the earnings date is an estimate.
    /// </summary>
    public bool? IsEarningsDateEstimate { get; set; }

    /// <summary>
    /// The trailing annual dividend rate for the quoted instrument.
    /// </summary>
    public double? TrailingAnnualDividendRate { get; set; }

    /// <summary>
    /// The trailing PE ratio for the quoted instrument.
    /// </summary>
    public double? TrailingPe { get; set; }

    /// <summary>
    /// The current dividend rate for the quoted instrument.
    /// </summary>
    public double? DividendRate { get; set; }

    /// <summary>
    /// The date of the next dividend payment.
    /// </summary>
    public DateTime? DividendDate { get; set; }

    /// <summary>
    /// The trailing annual dividend yield for the quoted instrument.
    /// </summary>
    public double? TrailingAnnualDividendYield { get; set; }

    /// <summary>
    /// The dividend yield for the quoted instrument.
    /// </summary>
    public double? DividendYield { get; set; }

    /// <summary>
    /// The trailing twelve months earnings per share (EPS).
    /// </summary>
    public double? EpsTrailingTwelveMonths { get; set; }

    /// <summary>
    /// The forward earnings per share (EPS) estimate.
    /// </summary>
    public double? EpsForward { get; set; }

    /// <summary>
    /// The earnings per share (EPS) for the current year.
    /// </summary>
    public double? EpsCurrentYear { get; set; }

    /// <summary>
    /// The price-to-earnings (PE) ratio for the current year.
    /// </summary>
    public double? PriceEpsCurrentYear { get; set; }

    /// <summary>
    /// The number of outstanding shares of the quoted instrument.
    /// </summary>
    public long? SharesOutstanding { get; set; }

    /// <summary>
    /// The book value of the quoted instrument.
    /// </summary>
    public double? BookValue { get; set; }

    /// <summary>
    /// The 50-day moving average for the quoted instrument.
    /// </summary>
    public double? FiftyDayAverage { get; set; }

    /// <summary>
    /// The change in the 50-day moving average for the quoted instrument.
    /// </summary>
    public double? FiftyDayAverageChange { get; set; }

    /// <summary>
    /// The hint value for the quote price.
    /// </summary>
    public double? PriceHint { get; set; }

    /// <summary>
    /// The percentage change in the post-market price of the quoted instrument.
    /// </summary>
    public double? PostMarketChangePercent { get; set; }

    /// <summary>
    /// The post-market time for the quoted instrument.
    /// </summary>
    public DateTime? PostMarketTime { get; set; }

    /// <summary>
    /// The post-market price of the quoted instrument.
    /// </summary>
    public double? PostMarketPrice { get; set; }

    /// <summary>
    /// The change in the post-market price of the quoted instrument.
    /// </summary>
    public double? PostMarketChange { get; set; }

    /// <summary>
    /// The change in the regular market price of the quoted instrument.
    /// </summary>
    public double? RegularMarketChange { get; set; }

    /// <summary>
    /// The time at which the regular market price was last updated.
    /// </summary>
    public DateTime? RegularMarketTime { get; set; }

    /// <summary>
    /// The highest price during the regular market day for the quoted instrument.
    /// </summary>
    public double? RegularMarketDayHigh { get; set; }

    /// <summary>
    /// The range of the regular market day for the quoted instrument.
    /// </summary>
    public string? RegularMarketDayRange { get; set; }

    /// <summary>
    /// The lowest price during the regular market day for the quoted instrument.
    /// </summary>
    public double? RegularMarketDayLow { get; set; }

    /// <summary>
    /// The volume of trades in the regular market for the quoted instrument.
    /// </summary>
    public long? RegularMarketVolume { get; set; }

    /// <summary>
    /// The price of the quoted instrument at the previous close.
    /// </summary>
    public double? RegularMarketPreviousClose { get; set; }

    /// <summary>
    /// The current bid price for the quoted instrument.
    /// </summary>
    public double? Bid { get; set; }

    /// <summary>
    /// The current ask price for the quoted instrument.
    /// </summary>
    public double? Ask { get; set; }

    /// <summary>
    /// The bid size for the quoted instrument.
    /// </summary>
    public long? BidSize { get; set; }

    /// <summary>
    /// The ask size for the quoted instrument.
    /// </summary>
    public long? AskSize { get; set; }

    /// <summary>
    /// The percentage change in the 50-day moving average for the quoted instrument.
    /// </summary>
    public double? FiftyDayAverageChangePercent { get; set; }

    /// <summary>
    /// The 200-day moving average for the quoted instrument.
    /// </summary>
    public double? TwoHundredDayAverage { get; set; }

    /// <summary>
    /// The change in the 200-day moving average for the quoted instrument.
    /// </summary>
    public double? TwoHundredDayAverageChange { get; set; }

    /// <summary>
    /// The percentage change in the 200-day moving average for the quoted instrument.
    /// </summary>
    public double? TwoHundredDayAverageChangePercent { get; set; }

    /// <summary>
    /// The market capitalization of the quoted instrument.
    /// </summary>
    public long? MarketCap { get; set; }

    /// <summary>
    /// The forward PE ratio for the quoted instrument.
    /// </summary>
    public double? ForwardPe { get; set; }

    /// <summary>
    /// The price-to-book ratio for the quoted instrument.
    /// </summary>
    public double? PriceToBook { get; set; }

    /// <summary>
    /// The source interval for the quote.
    /// </summary>
    public long? SourceInterval { get; set; }

    /// <summary>
    /// The exchange data delay for the quote.
    /// </summary>
    public long? ExchangeDataDelayedBy { get; set; }

    /// <summary>
    /// The average analyst rating for the quoted instrument.
    /// </summary>
    public string? AverageAnalystRating { get; set; }

    /// <summary>
    /// Indicates whether the instrument is tradeable.
    /// </summary>
    public bool? Tradeable { get; set; }

    /// <summary>
    /// Indicates whether the instrument is tradeable as a cryptocurrency.
    /// </summary>
    public bool? CryptoTradeable { get; set; }

    /// <summary>
    /// Indicates whether the quote has pre/post-market data.
    /// </summary>
    public bool? HasPrePostMarketData { get; set; }

    /// <summary>
    /// The date of the first trade for the quoted instrument.
    /// </summary>
    public DateTime? FirstTradeDate { get; set; }

    /// <summary>
    /// The display name for the quoted instrument.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// The symbol of the quoted instrument.
    /// </summary>
    public string? Symbol { get; set; }
}

using System;

namespace Finance.Net.Models.Yahoo;

/// <summary>
/// Represents the financial quote data of a stock.
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
    /// The short name.
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// The full name.
    /// </summary>
    public string? LongName { get; set; }

    /// <summary>
    /// The time zone of the exchange.
    /// </summary>
    public string? ExchangeTimezoneName { get; set; }

    /// <summary>
    /// The abbreviated time zone of the exchange.
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
    /// Indicates if ESG (Environmental, Social, Governance) data.
    /// </summary>
    public bool? EsgPopulated { get; set; }

    /// <summary>
    /// The percentage change in the regular market price.
    /// </summary>
    public double? RegularMarketChangePercent { get; set; }

    /// <summary>
    /// The regular market price.
    /// </summary>
    public double? RegularMarketPrice { get; set; }

    /// <summary>
    /// The market state (e.g., open or closed).
    /// </summary>
    public string? MarketState { get; set; }

    /// <summary>
    /// The full name of the exchange.
    /// </summary>
    public string? FullExchangeName { get; set; }

    /// <summary>
    /// The financial currency used for the quote.
    /// </summary>
    public string? FinancialCurrency { get; set; }

    /// <summary>
    /// The opening price.
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
    /// The change in the 52-week low price.
    /// </summary>
    public double? FiftyTwoWeekLowChange { get; set; }

    /// <summary>
    /// The percentage change in the 52-week low price.
    /// </summary>
    public double? FiftyTwoWeekLowChangePercent { get; set; }

    /// <summary>
    /// The 52-week price range.
    /// </summary>
    public string? FiftyTwoWeekRange { get; set; }

    /// <summary>
    /// The change in the 52-week high price.
    /// </summary>
    public double? FiftyTwoWeekHighChange { get; set; }

    /// <summary>
    /// The percentage change in the 52-week high price.
    /// </summary>
    public double? FiftyTwoWeekHighChangePercent { get; set; }

    /// <summary>
    /// The price at its 52-week low.
    /// </summary>
    public double? FiftyTwoWeekLow { get; set; }

    /// <summary>
    /// The price at its 52-week high.
    /// </summary>
    public double? FiftyTwoWeekHigh { get; set; }

    /// <summary>
    /// The percentage change in the 52-week price.
    /// </summary>
    public double? FiftyTwoWeekChangePercent { get; set; }

    /// <summary>
    /// The earnings date.
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
    /// The trailing annual dividend rate.
    /// </summary>
    public double? TrailingAnnualDividendRate { get; set; }

    /// <summary>
    /// The trailing PE ratio.
    /// </summary>
    public double? TrailingPe { get; set; }

    /// <summary>
    /// The current dividend rate.
    /// </summary>
    public double? DividendRate { get; set; }

    /// <summary>
    /// The date of the next dividend payment.
    /// </summary>
    public DateTime? DividendDate { get; set; }

    /// <summary>
    /// The trailing annual dividend yield.
    /// </summary>
    public double? TrailingAnnualDividendYield { get; set; }

    /// <summary>
    /// The dividend yield.
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
    /// The number of outstanding shares.
    /// </summary>
    public long? SharesOutstanding { get; set; }

    /// <summary>
    /// The book value.
    /// </summary>
    public double? BookValue { get; set; }

    /// <summary>
    /// The 50-day moving average.
    /// </summary>
    public double? FiftyDayAverage { get; set; }

    /// <summary>
    /// The change in the 50-day moving average.
    /// </summary>
    public double? FiftyDayAverageChange { get; set; }

    /// <summary>
    /// The hint value for the quote price.
    /// </summary>
    public double? PriceHint { get; set; }

    /// <summary>
    /// The percentage change in the post-market price.
    /// </summary>
    public double? PostMarketChangePercent { get; set; }

    /// <summary>
    /// The post-market time.
    /// </summary>
    public DateTime? PostMarketTime { get; set; }

    /// <summary>
    /// The post-market price.
    /// </summary>
    public double? PostMarketPrice { get; set; }

    /// <summary>
    /// The change in the post-market price.
    /// </summary>
    public double? PostMarketChange { get; set; }

    /// <summary>
    /// The change in the regular market price.
    /// </summary>
    public double? RegularMarketChange { get; set; }

    /// <summary>
    /// The time at which the regular market price was last updated.
    /// </summary>
    public DateTime? RegularMarketTime { get; set; }

    /// <summary>
    /// The highest price during the regular market day.
    /// </summary>
    public double? RegularMarketDayHigh { get; set; }

    /// <summary>
    /// The range of the regular market day.
    /// </summary>
    public string? RegularMarketDayRange { get; set; }

    /// <summary>
    /// The lowest price during the regular market day.
    /// </summary>
    public double? RegularMarketDayLow { get; set; }

    /// <summary>
    /// The volume of trades in the regular market.
    /// </summary>
    public long? RegularMarketVolume { get; set; }

    /// <summary>
    /// The price at the previous close.
    /// </summary>
    public double? RegularMarketPreviousClose { get; set; }

    /// <summary>
    /// The current bid price.
    /// </summary>
    public double? Bid { get; set; }

    /// <summary>
    /// The current ask price.
    /// </summary>
    public double? Ask { get; set; }

    /// <summary>
    /// The bid size.
    /// </summary>
    public long? BidSize { get; set; }

    /// <summary>
    /// The ask size.
    /// </summary>
    public long? AskSize { get; set; }

    /// <summary>
    /// The percentage change in the 50-day moving average.
    /// </summary>
    public double? FiftyDayAverageChangePercent { get; set; }

    /// <summary>
    /// The 200-day moving average.
    /// </summary>
    public double? TwoHundredDayAverage { get; set; }

    /// <summary>
    /// The change in the 200-day moving average.
    /// </summary>
    public double? TwoHundredDayAverageChange { get; set; }

    /// <summary>
    /// The percentage change in the 200-day moving average.
    /// </summary>
    public double? TwoHundredDayAverageChangePercent { get; set; }

    /// <summary>
    /// The market capitalization.
    /// </summary>
    public long? MarketCap { get; set; }

    /// <summary>
    /// The forward PE ratio.
    /// </summary>
    public double? ForwardPe { get; set; }

    /// <summary>
    /// The price-to-book ratio.
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
    /// The average analyst rating.
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
    /// The date of the first trade.
    /// </summary>
    public DateTime? FirstTradeDate { get; set; }

    /// <summary>
    /// The display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// The symbol.
    /// </summary>
    public string? Symbol { get; set; }
}

namespace Finance.Net;

#pragma warning disable S1075 // URIs should not be hardcoded
internal static class Constants
{
    // General
    public const string DefaultHttpRetryPolicy = "DefaultHttpRetryPolicy";

    // HTTP Headers
    public const string HeaderAccept = "Accept";
    public const string HeaderAcceptLanguage = "Accept-Language";
    public const string HeaderUserAgent = "User-Agent";
    public const string HeaderAcceptLanguageValue = "en-US,en;q=0.5";

    // Messages
    public const string ApiResponseLimitExceeded = "higher API call volume";
    public const string ValidationMessageAllFieldsEmpty = "All fields empty";

    #region Yahoo Finance

    public const string YahooHttpClientName = "FinanceNetYahooClient";
    public const int YahooCookieExpirationHours = 6;

    public const string YahooBaseUrl = "https://finance.yahoo.com";
    public const string YahooQuoteHtmlUrl = $"{YahooBaseUrl}/quote";
    public const string YahooAuthenticationUrl = "https://fc.yahoo.com";
    public const string YahooConsentUrl = "https://guce.yahoo.com/consent";
    public const string YahooConsentCollectUrl = "https://consent.yahoo.com/v2/collectConsent";
    public const string YahooCrumbApiUrl = "https://query1.finance.yahoo.com/v1/test/getcrumb";
    public const string YahooQuoteApiUrl = "https://query1.finance.yahoo.com/v7/finance/quote";

    #endregion

    #region Xetra

    public const string XetraHttpClientName = "FinanceNetXetraClient";
    public const string XetraInstrumentsUrl = "https://www.xetra.com/xetra-en/instruments/instruments";

    #endregion

    #region Alpha Vantage

    public const string AlphaVantageHttpClientName = "FinanceNetAlphaVantageClient";
    public const string AlphaVantageApiBaseUrl = "https://www.alphavantage.co";

    #endregion

    #region Datahub.io

    public const string DatahubIoHttpClientName = "DotNetFinanceDatahubIoClient";
    public const string DatahubSp500SymbolsUrl =
        "https://raw.githubusercontent.com/datasets/s-and-p-500-companies-financials/refs/heads/main/data/constituents-financials.csv";
    public const string DatahubNasdaqSymbolsUrl =
        "https://raw.githubusercontent.com/datasets/nasdaq-listings/refs/heads/main/data/nasdaq-listed-symbols.csv";

    #endregion
}
#pragma warning restore S1075 // URIs should not be hardcoded

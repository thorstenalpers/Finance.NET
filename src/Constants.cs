namespace Finance.Net;

internal static class Constants
{
    /// <summary> Base url for Yahoo UI content </summary>
    public static readonly string YahooBaseUrlQuoteHtml = "https://finance.yahoo.com/quote";

    /// <summary> Base url to get cookie for api calls </summary>
    public static readonly string YahooBaseUrlAuthentication = "https://fc.yahoo.com";

    /// <summary> Base url to get cookie for html calls </summary>
    public static readonly string YahooBaseUrlConsent = "https://guce.yahoo.com/consent";

    /// <summary> Base url to send consent cookie for html calls </summary>
    public static readonly string YahooBaseUrlConsentCollect = "https://consent.yahoo.com/v2/collectConsent";

    /// <summary> Base url for Yahoo crumb API calls </summary>
    public static readonly string YahooBaseUrlCrumbApi = "https://query1.finance.yahoo.com/v1/test/getcrumb";

    /// <summary> Base url for Yahoo quote API calls </summary>
    public static readonly string YahooBaseUrlQuoteApi = "https://query1.finance.yahoo.com/v7/finance/quote";

    /// <summary> Base url for Yahoo HTML </summary>
    public static readonly string YahooBaseUrlHtml = "https://finance.yahoo.com";

    /// <summary> Name of the Yahoo HttpClient used from HttpClientFactory </summary>
    public static readonly string YahooHttpClientName = "FinanceNetYahooClient";


    /// <summary> Name of the Xetra HttpClient used from HttpClientFactory </summary>
    public static readonly string XetraHttpClientName = "FinanceNetXetraClient";


    /// <summary> Base url for Alpha Vantage API calls </summary>
    public static readonly string AlphaVantageApiUrl = "https://www.alphavantage.co";

    /// <summary> Name of the AlphaVantage HttpClient used from HttpClientFactory </summary>
    public static readonly string AlphaVantageHttpClientName = "FinanceNetAlphaVantageClient";


    /// <summary> Name of the DatahubIo HttpClient used from HttpClientFactory </summary>
    public static readonly string DatahubIoHttpClientName = "DotNetFinanceDatahubIoClient";

    public static string XetraInstrumentsUrl = "https://www.xetra.com/xetra-en/instruments/instruments";
}
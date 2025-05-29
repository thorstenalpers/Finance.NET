using System;

namespace Finance.Net;

#pragma warning disable S1075 // URIs should not be hardcoded
internal static class Constants
{
    private static readonly string DefaultYahooBaseUrl = "https://finance.yahoo.com";
    private static readonly string DefaultXetraBaseUrl = "https://www.xetra.com";
    private static readonly string DefaultAlphaVantageBaseUrl = "https://www.alphavantage.co";
    private static readonly string DefaultDatahubIoBaseUrl = "https://raw.githubusercontent.com";

    private static readonly string BaseUrlYahoo = Environment.GetEnvironmentVariable("FINANCE_NET_YAHOO_BASE_URL") ?? DefaultYahooBaseUrl;
    private static readonly string BaseUrlXetra = Environment.GetEnvironmentVariable("FINANCE_NET_XETRA_BASE_URL") ?? DefaultXetraBaseUrl;
    private static readonly string BaseUrlAlphaVantage = Environment.GetEnvironmentVariable("FINANCE_NET_ALPHA_VANTAGE_BASE_URL") ?? DefaultAlphaVantageBaseUrl;
    private static readonly string BaseUrlDatahubIo = Environment.GetEnvironmentVariable("FINANCE_NET_DATAHUB_IO_BASE_URL") ?? DefaultDatahubIoBaseUrl;

    // Base URLs
    public static readonly string YahooBaseUrl = BaseUrlYahoo;
    public static readonly string YahooBaseUrlQuoteHtml = $"{YahooBaseUrl}/quote";
    public static readonly string YahooBaseUrlHtml = YahooBaseUrl;

    public static readonly string YahooBaseUrlAuthentication = $"{BaseUrlYahoo}/fc";
    public static readonly string YahooBaseUrlConsent = $"{BaseUrlYahoo}/guce/consent";
    public static readonly string YahooBaseUrlConsentCollect = $"{BaseUrlYahoo}/consent/v2/collectConsent";
    public static readonly string YahooBaseUrlCrumbApi = $"{BaseUrlYahoo}/query1.finance.yahoo.com/v1/test/getcrumb";
    public static readonly string YahooBaseUrlQuoteApi = $"{BaseUrlYahoo}/query1.finance.yahoo.com/v7/finance/quote";

    public static readonly string XetraBaseUrl = BaseUrlXetra;
    public static readonly string XetraInstrumentsUrl = $"{XetraBaseUrl}/xetra-en/instruments/instruments";

    public static readonly string AlphaVantageApiUrl = BaseUrlAlphaVantage;

    public static readonly string DatahubIoBaseUrl = BaseUrlDatahubIo;
    public static readonly string DatahubIoDownloadUrlSP500Symbols = $"{DatahubIoBaseUrl}/datasets/s-and-p-500-companies-financials/refs/heads/main/data/constituents-financials.csv";
    public static readonly string DatahubIoDownloadUrlNasdaqListedSymbols = $"{DatahubIoBaseUrl}/datasets/nasdaq-listings/refs/heads/main/data/nasdaq-listed-symbols.csv";

    // HttpClient names
    public static readonly string YahooHttpClientName = "FinanceNetYahooClient";
    public static readonly string XetraHttpClientName = "FinanceNetXetraClient";
    public static readonly string AlphaVantageHttpClientName = "FinanceNetAlphaVantageClient";
    public static readonly string DatahubIoHttpClientName = "DotNetFinanceDatahubIoClient";

    // Other constants
    public static readonly string DefaultHttpRetryPolicy = "DefaultHttpRetryPolicy";
    public static readonly string HeaderNameAccept = "Accept";
    public static readonly string HeaderNameAcceptLanguage = "Accept-Language";
    public static readonly string HeaderNameUserAgent = "User-Agent";
    public static readonly string ResponseApiLimitExceeded = "higher API call volume";
    public static readonly string ValidationMsgAllFieldsEmpty = "All fields empty";
    public static readonly string HeaderValueAcceptLanguage = "en-US,en;q=0.5";
    public static readonly int YahooCookieExpirationTimeInHours = 6;
}
#pragma warning restore S1075 // URIs should not be hardcoded

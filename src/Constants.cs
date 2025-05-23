﻿namespace Finance.Net;

internal static class Constants
{
    public const string DefaultHttpRetryPolicy = "DefaultHttpRetryPolicy";

    public const string HeaderNameAccept = "Accept";
    public const string HeaderNameAcceptLanguage = "Accept-Language";
    public const string HeaderNameUserAgent = "User-Agent";

    public const string ResponseApiLimitExceeded = "higher API call volume";

    public const string ValidationMsgAllFieldsEmpty = "All fields empty";
    public const string HeaderValueAcceptLanguage = "en-US,en;q=0.5";

    public const int YahooCookieExpirationTimeInHours = 6;

    /// <summary> Base url for Yahoo UI content </summary>
    public const string YahooBaseUrlQuoteHtml = "https://finance.yahoo.com/quote";

    /// <summary> Base url to get cookie for api calls </summary>
    public const string YahooBaseUrlAuthentication = "https://fc.yahoo.com";

    /// <summary> Base url to get cookie for html calls </summary>
    public const string YahooBaseUrlConsent = "https://guce.yahoo.com/consent";

    /// <summary> Base url to send consent cookie for html calls </summary>
    public const string YahooBaseUrlConsentCollect = "https://consent.yahoo.com/v2/collectConsent";

    /// <summary> Base url for Yahoo crumb API calls </summary>
    public const string YahooBaseUrlCrumbApi = "https://query1.finance.yahoo.com/v1/test/getcrumb";

    /// <summary> Base url for Yahoo quote API calls </summary>
    public const string YahooBaseUrlQuoteApi = "https://query1.finance.yahoo.com/v7/finance/quote";

    /// <summary> Base url for Yahoo HTML </summary>
    public const string YahooBaseUrlHtml = "https://finance.yahoo.com";

    /// <summary> Name of the Yahoo HttpClient used from HttpClientFactory </summary>
    public const string YahooHttpClientName = "FinanceNetYahooClient";


    public const string XetraInstrumentsUrl = "https://www.xetra.com/xetra-en/instruments/instruments";

    /// <summary> Name of the Xetra HttpClient used from HttpClientFactory </summary>
    public const string XetraHttpClientName = "FinanceNetXetraClient";


    /// <summary> Base url for Alpha Vantage API calls </summary>
    public const string AlphaVantageApiUrl = "https://www.alphavantage.co";

    /// <summary> Name of the AlphaVantage HttpClient used from HttpClientFactory </summary>
    public const string AlphaVantageHttpClientName = "FinanceNetAlphaVantageClient";


    /// <summary> Download URL of Datahub S&amp;P500 listed symbols </summary>
    public const string DatahubIoDownloadUrlSP500Symbols = "https://raw.githubusercontent.com/datasets/s-and-p-500-companies-financials/refs/heads/main/data/constituents-financials.csv";

    /// <summary> Download URL of Datahub nasdaq listed symbols </summary>
    public const string DatahubIoDownloadUrlNasdaqListedSymbols = "https://raw.githubusercontent.com/datasets/nasdaq-listings/refs/heads/main/data/nasdaq-listed-symbols.csv";

    /// <summary> Name of the DatahubIo HttpClient used from HttpClientFactory </summary>
    public const string DatahubIoHttpClientName = "DotNetFinanceDatahubIoClient";
}
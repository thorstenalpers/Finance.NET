using System.ComponentModel.DataAnnotations;

namespace DotNetFinance;
public class DotNetFinanceConfiguration
{
	/// <summary> Alpha Vantage API Key </summary>
	public string? AlphaVantageApiKey { get; set; }

	/// <summary> Default retries for failed http requests (caused by rate limits) </summary>
	[Required] public int HttpRetries = 3;

	/// <summary> Default HTTP timeout in seconds </summary>
	[Required] public int HttpTimeout = 30;

	/// <summary> time in hours after refrshing cookies </summary>
	[Required] public int YahooCookieRefreshTime = 6;

	/// <summary> Base url for Yahoo UI content </summary>
	[Required] public string YahooBaseUrlQuoteHtml = "https://finance.yahoo.com/quote";

	/// <summary> Base url to get cookie for api calls </summary>
	[Required] public string YahooBaseUrlAuthentication = "https://fc.yahoo.com";

	/// <summary> Base url to get cookie for html calls </summary>
	[Required] public string YahooBaseUrlConsent = "https://guce.yahoo.com/consent";

	/// <summary> Base url to send consent cookie for html calls </summary>
	[Required] public string YahooBaseUrlConsentCollect = "https://consent.yahoo.com/v2/collectConsent";

	/// <summary> Base url for Yahoo crumb API calls </summary>
	[Required] public string YahooBaseUrlCrumbApi = "https://query1.finance.yahoo.com/v1/test/getcrumb";

	/// <summary> Base url for Yahoo quote API calls </summary>
	[Required] public string YahooBaseUrlQuoteApi = "https://query1.finance.yahoo.com/v7/finance/quote";

	/// <summary> Base url for Yahoo quote API calls </summary>
	[Required] public string XetraDownloadUrlInstruments = "https://www.xetra.com/resource/blob/1528/76087c675c856fe7720917da03a62a34/data/t7-xetr-allTradableInstruments.csv";

	/// <summary> Download URL of DataHubIo S&P500 listed symbols </summary>
	[Required] public string DatahubIoDownloadUrlSP500Symbols = "https://raw.githubusercontent.com/datasets/s-and-p-500-companies-financials/refs/heads/main/data/constituents-financials.csv";

	/// <summary> Download URL of DataHubIo nasdaq listed symbols </summary>
	[Required] public string DatahubIoDownloadUrlNasdaqListedSymbols = "https://raw.githubusercontent.com/datasets/nasdaq-listings/refs/heads/main/data/nasdaq-listed-symbols.csv";

	/// <summary> Base url for Alpha Vantage API calls </summary>
	[Required] public string AlphaVantageApiUrl = "https://www.alphavantage.co";


	/// <summary> Base url for Yahoo HTML </summary>
	internal readonly string YahooBaseUrlHtml = "https://finance.yahoo.com";

	/// <summary> Name of the Yahoo HttpClient used from HttpClientFactory </summary>
	internal readonly string YahooHttpClientName = "FinanceNetYahooClient";

	/// <summary> Name of the Xetra HttpClient used from HttpClientFactory </summary>
	internal readonly string XetraHttpClientName = "FinanceNetXetraClient";

	/// <summary> Name of the AlphaVantage HttpClient used from HttpClientFactory </summary>
	internal readonly string AlphaVantageHttpClientName = "FinanceNetAlphaVantageClient";

	/// <summary> Name of the DatahubIo HttpClient used from HttpClientFactory </summary>
	internal readonly string DatahubIoHttpClientName = "FinanceNetDatahubIoClient";
}
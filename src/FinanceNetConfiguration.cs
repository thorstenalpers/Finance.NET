using System.ComponentModel.DataAnnotations;

namespace Finance.Net;
public class FinanceNetConfiguration
{
		/// <summary> Default retries for failed http requests (caused by rate limits) </summary>
		[Required] public int HttpRetryCount { get; set; } = 3;

		/// <summary> Default HTTP timeout in seconds </summary>
		[Required] public int HttpTimeout { get; set; } = 30;

		/// <summary> Interval in hours for renewing cookies </summary>
		[Required] public int YahooCookieExpirationTime { get; set; } = 6;

		/// <summary> Download URL of DataHubIo S&P500 listed symbols </summary>
		[Required] public string DatahubIoDownloadUrlSP500Symbols { get; set; } = "https://raw.githubusercontent.com/datasets/s-and-p-500-companies-financials/refs/heads/main/data/constituents-financials.csv";

		/// <summary> Download URL of DataHubIo nasdaq listed symbols </summary>
		[Required] public string DatahubIoDownloadUrlNasdaqListedSymbols { get; set; } = "https://raw.githubusercontent.com/datasets/nasdaq-listings/refs/heads/main/data/nasdaq-listed-symbols.csv";

		/// <summary> Alpha Vantage API Key </summary>
		public string? AlphaVantageApiKey { get; set; }
}

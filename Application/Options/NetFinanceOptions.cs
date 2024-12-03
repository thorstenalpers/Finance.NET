namespace NetFinance.Application.Options;

public class NetFinanceOptions
{
	public static string SectionName { get; } = "NetFinance";
	/// <summary>
	/// Base url for HTML calls
	/// </summary>
	public string BaseUrl_Html { get; set; } = "https://finance.yahoo.com/quote";

	/// <summary>
	/// Base url for auth API calls
	/// </summary>
	public string BaseUrl_Auth_Api { get; set; } = "https://fc.yahoo.com";

	/// <summary>
	/// Base url for crumb API calls
	/// </summary>
	public string BaseUrl_Crumb_Api { get; set; } = "https://query1.finance.yahoo.com/v1/test/getcrumb";

	/// <summary>
	/// Base url for quote API calls
	/// </summary>
	public string BaseUrl_Quote_Api { get; set; } = "https://query1.finance.yahoo.com/v7/finance/quote";

	/// <summary>
	/// Defaul timeout in seconds
	/// </summary>
	public int Timeout { get; set; } = 30;
}
using System.Net;

namespace NetFinance.Utilities
{
	internal interface IYahooCredentialsManager
	{
		bool AreValid();
		bool AreApiCredentialsValid();
		bool AreUiCredentialsValid();
		string GetApiUserAgent();
		string GetUiUserAgent();
		(string userAgent, CookieCollection? cookies, string? crumb) GetApiCredentials();
		(string userAgent, CookieCollection? cookies) GetUiCredentials();
		void SetApiCredentials(string? crumb, CookieContainer? cookieContainer);
		void SetUiCredentials(CookieContainer? cookieContainer);
	}
}
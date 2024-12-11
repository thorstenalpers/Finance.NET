using System.Net;

namespace NetFinance.Utilities
{
	internal interface IYahooSessionState
	{
		bool IsValid();
		string GetUserAgent();
		CookieContainer GetCookieContainer();
		string? GetCrumb();
		void SetCrumb(string crumb);
		void InvalidateSession();
	}
}
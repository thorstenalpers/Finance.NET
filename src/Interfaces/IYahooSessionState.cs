using System.Net;

namespace NetFinance.Utilities
{
	internal interface IYahooSessionState
	{
		bool AreValid();
		string GetUserAgent();
		CookieContainer GetCookieContainer();
		string? GetCrumb();
		void SetCrumb(string crumb);
		void InvalidateSession();
	}
}
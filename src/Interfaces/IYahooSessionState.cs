using System.Net;

namespace DotNetFinance.Interfaces
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
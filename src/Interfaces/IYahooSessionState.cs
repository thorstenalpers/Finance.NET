using System;
using System.Net;

namespace Finance.Net.Interfaces;

public interface IYahooSessionState
{
		bool IsValid();
		string GetUserAgent();
		CookieContainer GetCookieContainer();
		string? GetCrumb();
		void SetCrumb(string crumb, DateTime refreshTime);
		void InvalidateSession();
}
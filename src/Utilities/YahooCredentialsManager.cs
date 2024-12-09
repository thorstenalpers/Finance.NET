using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Options;

namespace NetFinance.Utilities;

internal class YahooCredentialsManager(IOptions<NetFinanceConfiguration> options) : IYahooCredentialsManager
{
	private readonly NetFinanceConfiguration _options = options.Value ?? throw new ArgumentNullException(nameof(options));
	private string _apiUserAgent = Helper.CreateRandomUserAgent();
	private string _uiUserAgent = Helper.CreateRandomUserAgent();
	private CookieContainer? _apiCookieContainer;
	private CookieContainer? _uiCookieContainer;
	private string? _crumb;
	private DateTime? _refreshTime;

	public string GetApiUserAgent()
	{
		return _apiUserAgent;
	}

	public string GetUiUserAgent()
	{
		return _uiUserAgent;
	}

	public (string userAgent, CookieCollection? cookies, string? crumb) GetApiCredentials()
	{
		return (_apiUserAgent, _apiCookieContainer?.GetCookies(new Uri(_options.Yahoo_BaseUrl_Html)), _crumb);
	}

	public (string userAgent, CookieCollection? cookies) GetUiCredentials()
	{
		return (_uiUserAgent, _uiCookieContainer?.GetCookies(new Uri(_options.Yahoo_BaseUrl_Html)));
	}

	public void SetApiCredentials(string? crumb, CookieContainer? cookieContainer)
	{
		_crumb = crumb;
		_apiCookieContainer = cookieContainer;
		_refreshTime = DateTime.UtcNow;
	}

	public void SetUiCredentials(CookieContainer? cookieContainer)
	{
		_uiCookieContainer = cookieContainer;
		_refreshTime = DateTime.UtcNow;
	}

	public bool AreValid()
	{
		return AreApiCredentialsValid() && AreUiCredentialsValid();
	}

	public bool AreApiCredentialsValid()
	{
		var cookies = _apiCookieContainer?.GetCookies(new Uri(_options.Yahoo_BaseUrl_Html));
		if (_refreshTime == null || cookies?.Count == null || cookies.Count == 0)
		{
			return false;
		}
		if (DateTime.UtcNow >= _refreshTime?.AddHours(_options.Yahoo_Cookie_RefreshTime))
		{
			// e.g. 10:00 >= 12:00 (09:00+3) = false, 10:00 >= 04:00 (01:00+3) = true
			return false;
		}
		var anyExpired = cookies.Cast<Cookie>().Where(e => e.Expires != default).Any(e => e.Expires < DateTime.UtcNow);
		return !anyExpired;
	}

	public bool AreUiCredentialsValid()
	{
		var cookies = _uiCookieContainer?.GetCookies(new Uri(_options.Yahoo_BaseUrl_Html));
		if (_refreshTime == null || cookies?.Count == null || cookies.Count == 0)
		{
			return false;
		}
		if (DateTime.UtcNow >= _refreshTime?.AddHours(_options.Yahoo_Cookie_RefreshTime))
		{
			// e.g. 10:00 >= 12:00 (09:00+3) = false, 10:00 >= 04:00 (01:00+3) = true
			return false;
		}
		var anyExpired = cookies.Cast<Cookie>().Where(e => e.Expires != default).Any(e => e.Expires < DateTime.UtcNow);
		return !anyExpired;
	}
}

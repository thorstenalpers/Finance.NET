﻿using System;
using System.Linq;
using System.Net;

using Finance.Net.Interfaces;

using Microsoft.Extensions.Options;

namespace Finance.Net.Utilities;

internal class YahooSessionState(IOptions<FinanceNetConfiguration> options) : IYahooSessionState
{
	private readonly FinanceNetConfiguration _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
	private readonly string _userAgent = Helper.CreateRandomUserAgent();
	private readonly CookieContainer _cookieContainer = new();
	private string? _crumb;
	private DateTime? _refreshTime;

	public string GetUserAgent()
	{
		return _userAgent;
	}

	public string? GetCrumb()
	{
		return _crumb;
	}

	public void SetCrumb(string crumb, DateTime refreshTime)
	{
		_crumb = crumb;
		_refreshTime = refreshTime;
	}

	public CookieContainer GetCookieContainer()
	{
		return _cookieContainer;
	}

	public bool IsValid()
	{
		var cookies = _cookieContainer.GetCookies(new Uri(Constants.YahooBaseUrlHtml));
		if (_refreshTime == null || cookies?.Count == null || cookies.Count == 0 || string.IsNullOrWhiteSpace(_crumb))
		{
			return false;
		}
		if (DateTime.UtcNow >= _refreshTime?.AddHours(_options.YahooCookieExpirationTime))
		{
			// e.g. 10:00 >= 12:00 (09:00+3) = false, 10:00 >= 04:00 (01:00+3) = true
			return false;
		}
		var anyExpired = cookies.Cast<Cookie>().Any(e => e.Expires != default && e.Expires < DateTime.UtcNow);
		return !anyExpired;
	}
}

using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Finance.Net.Interfaces;

namespace Finance.Net.Utilities;

internal class YahooSessionState : IYahooSessionState
{
    private string _userAgent = Helper.CreateRandomUserAgent();
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
        if (_refreshTime == null || (cookies?.Count ?? 0) == 0 || string.IsNullOrWhiteSpace(_crumb))
        {
            return false;
        }
        if (DateTime.UtcNow >= _refreshTime.Value.AddHours(Constants.YahooCookieExpirationTimeInHours))
        {
            // e.g. 10:00 >= 12:00 (09:00+3) = false, 10:00 >= 04:00 (01:00+3) = true
            return false;
        }
        var anyExpired = cookies.Cast<Cookie>().Any(e => e.Expires != default && e.Expires < DateTime.UtcNow);
        return !anyExpired;
    }

#pragma warning disable S3011
    public void InvalidateSession()
    {
        var fieldInfo = typeof(CookieContainer).GetField("m_domainTable", BindingFlags.NonPublic | BindingFlags.Instance);
        var domainTable = fieldInfo.GetValue(_cookieContainer);

        if (domainTable != null)
        {
            var clearMethod = domainTable.GetType().GetMethod("Clear", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            clearMethod.Invoke(domainTable, null);
        }

        _crumb = null;
        _refreshTime = null;
        _userAgent = Helper.CreateRandomUserAgent();
    }
#pragma warning restore S3011
}

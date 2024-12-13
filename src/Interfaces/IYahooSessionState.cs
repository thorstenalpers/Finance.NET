using System.Net;

namespace Finance.Net.Interfaces;

internal interface IYahooSessionState
{
    bool IsValid();
    string GetUserAgent();
    CookieContainer GetCookieContainer();
    string? GetCrumb();
    void SetCrumb(string crumb);
    void InvalidateSession();
}
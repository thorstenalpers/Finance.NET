using System;
using System.Net;

namespace Finance.Net.Interfaces;

/// <summary>
/// Represents the state of a Yahoo session, providing methods to check validity, manage cookies, and handle session crumbs.
/// </summary>
public interface IYahooSessionState
{
    /// <summary>
    /// Determines whether the current session state is valid.
    /// </summary>
    /// <returns><c>true</c> if the session is valid; otherwise, <c>false</c>.</returns>
    bool IsValid();

    /// <summary>
    /// Retrieves the user agent string associated with the session.
    /// </summary>
    /// <returns>The user agent string.</returns>
    string GetUserAgent();

    /// <summary>
    /// Retrieves the cookie container associated with the current session.
    /// </summary>
    /// <returns>A <see cref="CookieContainer"/> containing the session cookies.</returns>
    CookieContainer GetCookieContainer();

    /// <summary>
    /// Retrieves the current session crumb, which may be used for API authentication or state management.
    /// </summary>
    /// <returns>The session crumb as a string, or <c>null</c> if no crumb is set.</returns>
    string? GetCrumb();

    /// <summary>
    /// Sets the session crumb and updates the refresh time.
    /// </summary>
    /// <param name="crumb">The session crumb to set.</param>
    /// <param name="refreshTime">The time at which the session crumb was refreshed.</param>
    void SetCrumb(string crumb, DateTime refreshTime);

    /// <summary>
    /// Invalidates the session by clearing cookies, etc
    /// </summary>
    void InvalidateSession();
}

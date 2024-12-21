using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Finance.Net.Interfaces;

/// <summary>
/// Manages the session for interacting with Yahoo's APIs, including session refresh, cookie management, and user agent retrieval.
/// </summary>
public interface IYahooSessionManager
{
    /// <summary>
    /// Refreshes the Yahoo session asynchronously.
    /// </summary>
    /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RefreshSessionAsync(CancellationToken token = default);

    /// <summary>
    /// Retrieves the API crumb, which is typically used for authentication or API calls.
    /// </summary>
    /// <returns>The API crumb as a string, or <c>null</c> if it is not available.</returns>
    string? GetApiCrumb();

    /// <summary>
    /// Retrieves the cookies associated with the current session.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="Cookie"/> objects representing the session cookies.</returns>
    IEnumerable<Cookie> GetCookies();

    /// <summary>
    /// Retrieves the user agent string for the current session.
    /// </summary>
    /// <returns>The user agent string.</returns>
    string GetUserAgent();
}


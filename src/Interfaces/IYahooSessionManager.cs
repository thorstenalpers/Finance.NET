using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Finance.Net.Interfaces
{
    public interface IYahooSessionManager
    {
        Task RefreshSessionAsync(CancellationToken token = default);
        string? GetApiCrumb();
        IEnumerable<Cookie> GetCookies();
        string GetUserAgent();
    }
}

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetFinance.Interfaces
{
	public interface IYahooSessionManager
	{
		Task RefreshSessionAsync(CancellationToken token = default, bool forceRefresh = false);
		string? GetApiCrumb();
		IEnumerable<Cookie> GetCookies();
		string GetUserAgent();
	}
}

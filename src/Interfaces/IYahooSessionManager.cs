using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetFinance.Interfaces
{
	public interface IYahooSessionManager
	{
		Task<(string userAgent, CookieCollection? cookies, string? crumb)> GetApiCredentials(CancellationToken token = default);
		Task<(string userAgent, CookieCollection? cookies)> GetUiCredentials(CancellationToken token = default);
		string GetUiUserAgent();
	}
}

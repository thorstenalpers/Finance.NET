using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetFinance.Application.Utilities;

public interface IYahooSession
{
	public Task<(string crumb, Cookie cookie)> GetSessionStateAsync(CancellationToken token = default);

}

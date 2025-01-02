using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Models.Xetra;

namespace Finance.Net.Interfaces;

/// <summary>
/// Provides an interface for accessing Xetra market data services.
/// </summary>
public interface IXetraService
{
    /// <summary>
    /// Retrieves a collection of instruments available in the Xetra market.
    /// </summary>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains an enumerable of <see cref="Instrument"/>.</returns>
    Task<IEnumerable<Instrument>> GetInstrumentsAsync(CancellationToken token = default);
}
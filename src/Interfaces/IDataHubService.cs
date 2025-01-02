using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Models.DataHub;

namespace Finance.Net.Interfaces;

/// <summary>
/// Represents a service for interacting with the Datahub API.
/// </summary>
public interface IDataHubService
{
    /// <summary>
    /// Asynchronously retrieves a list of Nasdaq instruments from the Datahub API.
    /// </summary>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains an enumerable collection of <see cref="NasdaqInstrument"/>.</returns>
    Task<IEnumerable<NasdaqInstrument>> GetNasdaqInstrumentsAsync(CancellationToken token = default);

    /// <summary>
    /// Asynchronously retrieves a list of S&amp;P 500 instruments from the Datahub API.
    /// </summary>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains an enumerable collection of <see cref="Sp500Instrument"/>.</returns>
    Task<IEnumerable<Sp500Instrument>> GetSp500InstrumentsAsync(CancellationToken token = default);
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Enums;
using Finance.Net.Models.AlphaVantage;

namespace Finance.Net.Interfaces;

/// <summary>
/// Represents a service for interacting with the AlphaVantage API.
/// </summary>
public interface IAlphaVantageService
{
    /// <summary>
    /// Retrieves the instrument overview.
    /// </summary>
    /// <param name="symbol">The symbol (e.g., "AAPL" for Apple).</param>
    /// <param name="token">An optional cancellation token.</param>
    /// <returns>The task result contains the instrument overview.</returns>
    Task<InstrumentOverview?> GetOverviewAsync(string symbol, CancellationToken token = default);

    /// <summary>
    /// Retrieves the historical daily records.
    /// </summary>
    /// <param name="symbol">The symbol (e.g., "AAPL" for Apple).</param>
    /// <param name="startDate">Optional start date. If not provided, current date -7 days.</param>
    /// <param name="endDate">Optional end date. If not provided, current date.</param>
    /// <param name="token">An optional cancellation token.</param>
    /// <returns>The task result contains an enumerable collection of <see cref="Record"/>.</returns>
    Task<IEnumerable<Record>> GetRecordsAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null, CancellationToken token = default);

    /// <summary>
    /// Retrieves historical daily forex records.
    /// </summary>
    /// <param name="currency1">From (e.g., "USD").</param>
    /// <param name="currency2">To (e.g., "EUR").</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">Optional end date. If not provided, the current date will be used.</param>
    /// <param name="token">An optional cancellation token.</param>
    /// <returns>The task result contains an enumerable collection of <see cref="ForexRecord"/>.</returns>
    Task<IEnumerable<ForexRecord>> GetForexRecordsAsync(string currency1, string currency2, DateTime startDate, DateTime? endDate = null, CancellationToken token = default);

    /// <summary>
    /// Retrieves intraday records.
    /// </summary>
    /// <param name="symbol">The symbol (e.g., "AAPL" for Apple).</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">Optional end date. If not provided, the current date.</param>
    /// <param name="interval">The time interval between data points. Default is 15 minutes.</param>
    /// <param name="token">An optional cancellation token.</param>
    /// <returns>The task result contains an enumerable collection of <see cref="IntradayRecord"/>.</returns>
    Task<IEnumerable<IntradayRecord>> GetIntradayRecordsAsync(string symbol, DateTime startDate, DateTime? endDate = null, EInterval interval = EInterval.Interval_15Min, CancellationToken token = default);
}
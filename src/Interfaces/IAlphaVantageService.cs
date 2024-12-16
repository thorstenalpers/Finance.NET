using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Models.AlphaVantage;

namespace Finance.Net.Interfaces;

/// <summary>
/// Represents a service for interacting with the AlphaVantage API.
/// Provides methods for retrieving company data, stock records, forex data, and intraday information.
/// </summary>
public interface IAlphaVantageService
{
    /// <summary>
    /// Asynchronously retrieves the company overview for a specific symbol from AlphaVantage.
    /// </summary>
    /// <param name="symbol">The symbol of the company (e.g., "AAPL" for Apple).</param>
    /// <param name="token">A <see cref="CancellationToken"/> to allow cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the company info data for the given symbol, or null if the data is unavailable.</returns>
    Task<CompanyOverview?> GetCompanyOverviewAsync(string symbol, CancellationToken token = default);

    /// <summary>
    /// Asynchronously retrieves the historical daily stock records for a specific symbol from AlphaVantage.
    /// </summary>
    /// <param name="symbol">The symbol of the company (e.g., "AAPL" for Apple).</param>
    /// <param name="startDate">The start date for retrieving daily records.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="HistoryRecord"/> objects for the given symbol and date range.</returns>
    Task<IEnumerable<HistoryRecord>> GetHistoryRecordsAsync(string symbol, DateTime startDate);

    /// <summary>
    /// Asynchronously retrieves the historical daily stock records for a specific symbol from AlphaVantage.
    /// </summary>
    /// <param name="symbol">The symbol of the company (e.g., "AAPL" for Apple).</param>
    /// <param name="startDate">The start date for retrieving daily records.</param>
    /// <param name="endDate">Optional end date for retrieving daily records. If not provided, the current date will be used.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to allow cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="HistoryRecord"/> objects for the given symbol and date range.</returns>
    Task<IEnumerable<HistoryRecord>> GetHistoryRecordsAsync(string symbol, DateTime startDate, DateTime endDate, CancellationToken token = default);

    /// <summary>
    /// Asynchronously retrieves historical daily forex records for a currency pair from AlphaVantage.
    /// </summary>
    /// <param name="currency1">The first currency in the pair (e.g., "USD").</param>
    /// <param name="currency2">The second currency in the pair (e.g., "EUR").</param>
    /// <param name="startDate">The start date for retrieving forex records.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="HistoryForexRecord"/> objects for the given currency pair and date range.</returns>
    Task<IEnumerable<HistoryForexRecord>> GetHistoryForexRecordsAsync(string currency1, string currency2, DateTime startDate);

    /// <summary>
    /// Asynchronously retrieves historical daily forex records for a currency pair from AlphaVantage.
    /// </summary>
    /// <param name="currency1">The first currency in the pair (e.g., "USD").</param>
    /// <param name="currency2">The second currency in the pair (e.g., "EUR").</param>
    /// <param name="startDate">The start date for retrieving forex records.</param>
    /// <param name="endDate">Optional end date for retrieving forex records. If not provided, the current date will be used.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to allow cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="HistoryForexRecord"/> objects for the given currency pair and date range.</returns>
    Task<IEnumerable<HistoryForexRecord>> GetHistoryForexRecordsAsync(string currency1, string currency2, DateTime startDate, DateTime endDate, CancellationToken token = default);

    /// <summary>
    /// Asynchronously retrieves intraday stock records for a specific symbol from AlphaVantage.
    /// </summary>
    /// <param name="symbol">The symbol of the company (e.g., "AAPL" for Apple).</param>
    /// <param name="startDate">The start date for retrieving intraday records.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="HistoryIntradayRecord"/> objects for the given symbol, date range, and interval.</returns>
    Task<IEnumerable<HistoryIntradayRecord>> GetHistoryIntradayRecordsAsync(string symbol, DateTime startDate);

    /// <summary>
    /// Asynchronously retrieves intraday stock records for a specific symbol from AlphaVantage.
    /// </summary>
    /// <param name="symbol">The symbol of the company (e.g., "AAPL" for Apple).</param>
    /// <param name="startDate">The start date for retrieving intraday records.</param>
    /// <param name="endDate">Optional end date for retrieving intraday records. If not provided, the current date will be used.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="HistoryIntradayRecord"/> objects for the given symbol, date range, and interval.</returns>
    Task<IEnumerable<HistoryIntradayRecord>> GetHistoryIntradayRecordsAsync(string symbol, DateTime startDate, DateTime endDate);
    /// <summary>
    /// Asynchronously retrieves intraday stock records for a specific symbol from AlphaVantage.
    /// </summary>
    /// <param name="symbol">The symbol of the company (e.g., "AAPL" for Apple).</param>
    /// <param name="startDate">The start date for retrieving intraday records.</param>
    /// <param name="endDate">Optional end date for retrieving intraday records. If not provided, the current date will be used.</param>
    /// <param name="interval">The time interval between data points (1min, 5min, 15min, 30min, 60min). Default is 15 minutes.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to allow cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="HistoryIntradayRecord"/> objects for the given symbol, date range, and interval.</returns>
    Task<IEnumerable<HistoryIntradayRecord>> GetHistoryIntradayRecordsAsync(string symbol, DateTime startDate, DateTime endDate, EInterval interval, CancellationToken token = default);
}
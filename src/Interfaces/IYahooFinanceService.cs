using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Finance.Net.Enums;
using Finance.Net.Models.Yahoo;

namespace Finance.Net.Interfaces;

/// <summary>
/// Represents a service for interacting with the Yahoo! Finance API.
/// </summary>
public interface IYahooFinanceService
{
    /// <summary>
    /// Retrieves a collection of instruments.
    /// </summary>
    /// <param name="filterByType"> An optional filter to specify the asset type. If not provided, all types will be included.</param>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains an enumerable collection of <see cref="Instrument"/>.</returns>
    Task<IEnumerable<Instrument>> GetInstrumentsAsync(EAssetType? filterByType = null, CancellationToken token = default);

    /// <summary>
    /// Retrieves a profile.
    /// </summary>
    /// <param name="symbol">The symbol of the quote (e.g., "AAPL" for Apple).</param>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains the profile.</returns>
    Task<Profile> GetProfileAsync(string symbol, CancellationToken token = default);

    /// <summary>
    /// Retrieves the summary.
    /// </summary>
    /// <param name="symbol">The symbol of the quote (e.g., "AAPL" for Apple).</param>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains the summary.</returns>
    Task<Summary> GetSummaryAsync(string symbol, CancellationToken token = default);

    /// <summary>
    /// Retrieves the financial reports.
    /// </summary>
    /// <param name="symbol">The symbol of the quote (e.g., "AAPL" for Apple).</param>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains the financial reports by label.</returns>
    Task<Dictionary<string, FinancialReport>> GetFinancialsAsync(string symbol, CancellationToken token = default);

    /// <summary>
    /// Retrieves historical records.
    /// </summary>
    /// <param name="symbol">The symbol of the quote (e.g., "AAPL" for Apple).</param>
    /// <param name="startDate">Optional start date for retrieving historical records. If not provided, current date -7 days.</param>
    /// <param name="endDate">Optional end date for retrieving historical records. If not provided, current date.</param>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains an enumerable of record.</returns>
    Task<IEnumerable<Record>> GetRecordsAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null, CancellationToken token = default);

    /// <summary>
    /// Retrieves a single quote.
    /// </summary>
    /// <param name="symbol">The symbol of the quote (e.g., "AAPL" for Apple).</param>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains the quote.</returns>
    Task<Quote> GetQuoteAsync(string symbol, CancellationToken token = default);

    /// <summary>
    /// Retrieves multiple quotes.
    /// </summary>
    /// <param name="symbols">A list of symbols for the quotes to retrieve data for.</param>
    /// <param name="token">An optional cancellation token to cancel the operation if needed.</param>
    /// <returns>The task result contains an enumerable list of quotes.</returns>
    Task<IEnumerable<Quote>> GetQuotesAsync(List<string> symbols, CancellationToken token = default);

    /// <summary>
    /// Invalidates the session by clearing cookies, etc
    /// </summary>
    void InvalidateSession();
}
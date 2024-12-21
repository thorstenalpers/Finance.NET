namespace Finance.Net.Models.DatahubIo;

/// <summary>
/// Represents a financial instrument listed on the Nasdaq stock exchange, including details such as symbol, company name, security name, and other related attributes.
/// </summary>
public record NasdaqInstrument
{
    /// <summary>
    /// The symbol of the instrument.
    /// </summary>
    /// <value>The symbol of the instrument, or <c>null</c> if not available.</value>
    public string? Symbol { get; set; }

    /// <summary>
    /// The company name associated with the instrument.
    /// </summary>
    /// <value>The company name, or <c>null</c> if not available.</value>
    public string? CompanyName { get; set; }

    /// <summary>
    /// The name of the security associated with the instrument.
    /// </summary>
    /// <value>The security name, or <c>null</c> if not available.</value>
    public string? SecurityName { get; set; }

    /// <summary>
    /// The market category of the instrument.
    /// </summary>
    /// <value>The market category, or <c>null</c> if not available.</value>
    public string? MarketCategory { get; set; }

    /// <summary>
    /// The test issue flag of the instrument.
    /// </summary>
    /// <value>The test issue flag, or <c>null</c> if not specified.</value>
    public string? TestIssue { get; set; }

    /// <summary>
    /// The financial status of the instrument.
    /// </summary>
    /// <value>The financial status, or <c>null</c> if not available.</value>
    public string? FinancialStatus { get; set; }

    /// <summary>
    /// The round lot size for the instrument.
    /// </summary>
    /// <value>The round lot size, or <c>null</c> if not specified.</value>
    public int? RoundLotSize { get; set; }

    /// <summary>
    /// The Exchange-Traded Fund (ETF) information, if applicable.
    /// </summary>
    /// <value>The ETF details, or <c>null</c> if not applicable.</value>
    public string? ETF { get; set; }

    /// <summary>
    /// The NextShares information, if applicable.
    /// </summary>
    /// <value>The NextShares details, or <c>null</c> if not applicable.</value>
    public string? NextShares { get; set; }
}

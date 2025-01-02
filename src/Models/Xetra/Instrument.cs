namespace Finance.Net.Models.Xetra;

/// <summary>
/// Represents detailed information about a financial instrument or symbol.
/// </summary>
public record Instrument
{
    /// <summary>
    /// Gets or sets the unique symbol identifier for the instrument.
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Gets or sets the status of the instrument (e.g., active or inactive).
    /// </summary>
    public string? InstrumentStatus { get; set; }

    /// <summary>
    /// Gets or sets the full name of the instrument.
    /// </summary>
    public string? InstrumentName { get; set; }

    /// <summary>
    /// Gets or sets the International Securities Identification Number (ISIN) of the instrument.
    /// </summary>
    public string? ISIN { get; set; }

    /// <summary>
    /// Gets or sets the Wertpapierkennnummer (WKN), a German securities identification number.
    /// </summary>
    public string? WKN { get; set; }

    /// <summary>
    /// Gets or sets the mnemonic code of the instrument.
    /// </summary>
    public string? Mnemonic { get; set; }

    /// <summary>
    /// Gets or sets the type of the instrument (e.g., CS, ETF, ETN).
    /// </summary>
    public string? InstrumentType { get; set; }

    /// <summary>
    /// Gets or sets the currency in which the instrument is traded.
    /// </summary>
    public string? Currency { get; set; }
}
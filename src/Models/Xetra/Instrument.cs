namespace Finance.Net.Models.Xetra;
/// <summary>
/// Represents a financial instrument with various attributes such as symbol, status, and identification codes.
/// </summary>
public record Instrument
{
    /// <summary>
    /// The symbol or ticker of the financial instrument.
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// The current status of the instrument (e.g., active, inactive).
    /// </summary>
    public string? InstrumentStatus { get; set; }

    /// <summary>
    /// The name of the financial instrument.
    /// </summary>
    public string? InstrumentName { get; set; }

    /// <summary>
    /// The International Securities Identification Number (ISIN) for the instrument.
    /// </summary>
    public string? ISIN { get; set; }

    /// <summary>
    /// The Wertpapierkennnummer (WKN) of the instrument, a German securities identification number.
    /// </summary>
    public string? WKN { get; set; }

    /// <summary>
    /// The mnemonic or shorthand code representing the financial instrument.
    /// </summary>
    public string? Mnemonic { get; set; }

    /// <summary>
    /// The type of the financial instrument (e.g., stock, bond, option).
    /// </summary>
    public string? InstrumentType { get; set; }

    /// <summary>
    /// The currency in which the instrument is traded.
    /// </summary>
    public string? Currency { get; set; }

    internal string? ProductID { get; set; }
    internal string? TickSize1 { get; set; }
}

using Finance.Net.Enums;

namespace Finance.Net.Models.Yahoo;

/// <summary>
/// Represents a symbol info
/// </summary>
public record SymbolInfo
{
    /// <summary>
    /// The symbol or ticker of the financial instrument.
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// The type of the financial instrument.
    /// </summary>
    public EInstrumentType? InstrumentType { get; set; }
}

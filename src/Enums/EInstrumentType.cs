namespace Finance.Net.Enums;

/// <summary>
/// Represents the different types for financial instruments.
/// </summary>
public enum EInstrumentType
{
    /// <summary>
    /// Represents a stock instrument.
    /// </summary>
    Stock = 1,

    /// <summary>
    /// Represents an exchange-traded fund (ETF) instrument.
    /// </summary>
    ETF = 2,

    /// <summary>
    /// Represents a foreign exchange (Forex) instrument.
    /// </summary>
    Forex = 3,

    /// <summary>
    /// Represents a cryptocurrency instrument.
    /// </summary>
    Crypto = 4,

    /// <summary>
    /// Represents an index instrument.
    /// </summary>
    Index = 5,
}
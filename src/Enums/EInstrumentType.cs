namespace Finance.Net.Enums;

/// <summary>
/// Represents the different types for financial instruments.
/// </summary>
public enum EInstrumentType
{
    /// <summary>
    /// A stock instrument.
    /// </summary>
    Stock = 1,

    /// <summary>
    /// An exchange-traded fund (ETF) instrument.
    /// </summary>
    ETF = 2,

    /// <summary>
    /// A foreign exchange (Forex) instrument.
    /// </summary>
    Forex = 3,

    /// <summary>
    /// A cryptocurrency instrument.
    /// </summary>
    Crypto = 4,

    /// <summary>
    /// An index instrument.
    /// </summary>
    Index = 5,
}
using Finance.Net.Models.Xetra;
using Finance.Net.Models.Xetra.Dto;

namespace Finance.Net.Mappings;

internal static class XetraInstrumentMapper
{
    public static Instrument ToInstrument(this InstrumentItem src) => new()
    {
        Symbol = src.Mnemonic + ".DE",
        InstrumentStatus = src.InstrumentStatus,
        InstrumentName = src.Instrument,
        ISIN = src.ISIN,
        WKN = src.WKN,
        Mnemonic = src.Mnemonic,
        InstrumentType = src.InstrumentType,
        Currency = src.Currency,
    };
}

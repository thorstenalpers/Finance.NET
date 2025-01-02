using CsvHelper.Configuration;
using Finance.Net.Models.DataHub;

namespace Finance.Net.Mappings;

internal class NasdaqInstrumentMapping : ClassMap<NasdaqInstrument>
{
    public NasdaqInstrumentMapping()
    {
        Map(m => m.Symbol).Name("Symbol");
        Map(m => m.Name).Name("Company Name");
    }
}

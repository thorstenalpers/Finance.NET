using CsvHelper;
using Finance.Net.Models.DataHub;

namespace Finance.Net.Mappings;

[CsvRecordMap(typeof(NasdaqInstrument))]
internal partial class NasdaqInstrumentMapping
{
    [CsvColumn("Symbol")]
    public string? Symbol { get; set; }

    [CsvColumn("Company Name")]
    public string? Name { get; set; }
}

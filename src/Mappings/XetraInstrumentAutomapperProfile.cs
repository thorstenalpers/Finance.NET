using Finance.Net.Models.Xetra;
using Finance.Net.Models.Xetra.Dto;

namespace Finance.Net.Mappings;

internal class XetraInstrumentAutomapperProfile : AutoMapper.Profile
{
	public XetraInstrumentAutomapperProfile()
	{
		CreateMap<InstrumentItem, Instrument>()
			.ForMember(dest => dest.InstrumentName, opt => opt.MapFrom(src => src.Instrument))
			.ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Mnemonic + ".DE"));
	}
}
using System.Diagnostics.CodeAnalysis;
using DotNetFinance.Models.Xetra;
using DotNetFinance.Models.Xetra.Dto;

namespace DotNetFinance.Mappings;

[ExcludeFromCodeCoverage]
internal class XetraInstrumentAutomapperProfile : AutoMapper.Profile
{
	public XetraInstrumentAutomapperProfile()
	{
		CreateMap<InstrumentItem, Instrument>()
			.ForMember(dest => dest.InstrumentName, opt => opt.MapFrom(src => src.Instrument))
			.ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Mnemonic + ".DE"));
	}
}
using Finance.Net.Models.Yahoo;
using Finance.Net.Models.Yahoo.Dtos;
using Finance.Net.Utilities;

namespace Finance.Net.Mappings;

internal class YahooQuoteAutomapperProfile : AutoMapper.Profile
{
	public YahooQuoteAutomapperProfile()
	{
		CreateMap<QuoteResponse, Quote>()
				.ForMember(dest => dest.FirstTradeDate, opt => opt.MapFrom(src => Helper.UnixMillisecsToDate(src.FirstTradeDateMilliseconds)))
				.ForMember(dest => dest.RegularMarketTime, opt => opt.MapFrom(src => Helper.UnixToDateTime(src.RegularMarketTime)))
				.ForMember(dest => dest.PostMarketTime, opt => opt.MapFrom(src => Helper.UnixToDateTime(src.PostMarketTime)))
				.ForMember(dest => dest.DividendDate, opt => opt.MapFrom(src => Helper.UnixToDateTime(src.DividendDate)))
				.ForMember(dest => dest.EarningsDate, opt => opt.MapFrom(src => Helper.UnixToDateTime(src.EarningsTimestamp)))
				.ForMember(dest => dest.EarningsDateStart, opt => opt.MapFrom(src => Helper.UnixToDateTime(src.EarningsTimestampStart)))
				.ForMember(dest => dest.EarningsDateEnd, opt => opt.MapFrom(src => Helper.UnixToDateTime(src.EarningsTimestampEnd)))
				.ForMember(dest => dest.EarningsCallDateStart, opt => opt.MapFrom(src => Helper.UnixToDateTime(src.EarningsCallTimestampStart)))
				.ForMember(dest => dest.EarningsCallDateEnd, opt => opt.MapFrom(src => Helper.UnixToDateTime(src.EarningsCallTimestampEnd)));
	}
}

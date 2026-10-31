using CsvHelper;
using Finance.Net.Models.Xetra.Dto;

namespace Finance.Net.Mappings;

[CsvRecordMap(typeof(InstrumentItem))]
internal partial class XetraInstrumentsMapping
{
    [CsvColumn("Product Status")]
    public string? ProductStatus { get; set; }

    [CsvColumn("Instrument Status")]
    public string? InstrumentStatus { get; set; }

    [CsvColumn("Instrument")]
    public string? Instrument { get; set; }

    [CsvColumn("ISIN")]
    public string? ISIN { get; set; }

    [CsvColumn("Product ID")]
    public string? ProductID { get; set; }

    [CsvColumn("Instrument ID")]
    public string? InstrumentID { get; set; }

    [CsvColumn("WKN")]
    public string? WKN { get; set; }

    [CsvColumn("Mnemonic")]
    public string? Mnemonic { get; set; }

    [CsvColumn("MIC Code")]
    public string? MICCode { get; set; }

    [CsvColumn("CCP eligible Code")]
    public string? CCPeligibleCode { get; set; }

    [CsvColumn("Trading Model Type")]
    public string? TradingModelType { get; set; }

    [CsvColumn("Product Assignment Group")]
    public string? ProductAssignmentGroup { get; set; }

    [CsvColumn("Product Assignment Group Description")]
    public string? ProductAssignmentGroupDescription { get; set; }

    [CsvColumn("Designated Sponsor Member ID")]
    public string? DesignatedSponsorMemberID { get; set; }

    [CsvColumn("Designated Sponsor")]
    public string? DesignatedSponsor { get; set; }

    [CsvColumn("Price Range Value")]
    public string? PriceRangeValue { get; set; }

    [CsvColumn("Price Range Percentage")]
    public string? PriceRangePercentage { get; set; }

    [CsvColumn("Minimum Quote Size")]
    public string? MinimumQuoteSize { get; set; }

    [CsvColumn("Instrument Type")]
    public string? InstrumentType { get; set; }

    [CsvColumn("Tick Size 1")]
    public string? TickSize1 { get; set; }

    [CsvColumn("Upper Price Limit Max")]
    public string? UpperPriceLimitMax { get; set; }

    [CsvColumn("Tick Size 2")]
    public double TickSize2 { get; set; }

    [CsvColumn("Upper Price Limit 2")]
    public double UpperPriceLimit2 { get; set; }

    [CsvColumn("Tick Size 3")]
    public double TickSize3 { get; set; }

    [CsvColumn("Upper Price Limit 3")]
    public double UpperPriceLimit3 { get; set; }

    [CsvColumn("Tick Size 4")]
    public string? TickSize4 { get; set; }

    [CsvColumn("Upper Price Limit 4")]
    public string? UpperPriceLimit4 { get; set; }

    [CsvColumn("Tick Size 5")]
    public string? TickSize5 { get; set; }

    [CsvColumn("Upper Price Limit 5")]
    public string? UpperPriceLimit5 { get; set; }

    [CsvColumn("Tick Size 6")]
    public string? TickSize6 { get; set; }

    [CsvColumn("Upper Price Limit 6")]
    public string? UpperPriceLimit6 { get; set; }

    [CsvColumn("Tick Size 7")]
    public string? TickSize7 { get; set; }

    [CsvColumn("Upper Price Limit 7")]
    public string? UpperPriceLimit7 { get; set; }

    [CsvColumn("Tick Size 8")]
    public string? TickSize8 { get; set; }

    [CsvColumn("Upper Price Limit 8")]
    public string? UpperPriceLimit8 { get; set; }

    [CsvColumn("Tick Size 9")]
    public string? TickSize9 { get; set; }

    [CsvColumn("Upper Price Limit 9")]
    public string? UpperPriceLimit9 { get; set; }

    [CsvColumn("Tick Size 10")]
    public string? TickSize10 { get; set; }

    [CsvColumn("Upper Price Limit 10")]
    public string? UpperPriceLimit10 { get; set; }

    [CsvColumn("Tick Size 11")]
    public string? TickSize11 { get; set; }

    [CsvColumn("Upper Price Limit 11")]
    public string? UpperPriceLimit11 { get; set; }

    [CsvColumn("Tick Size 12")]
    public string? TickSize12 { get; set; }

    [CsvColumn("Upper Price Limit 12")]
    public string? UpperPriceLimit12 { get; set; }

    [CsvColumn("Tick Size 13")]
    public string? TickSize13 { get; set; }

    [CsvColumn("Upper Price Limit 13")]
    public string? UpperPriceLimit13 { get; set; }

    [CsvColumn("Tick Size 14")]
    public string? TickSize14 { get; set; }

    [CsvColumn("Upper Price Limit 14")]
    public string? UpperPriceLimit14 { get; set; }

    [CsvColumn("Tick Size 15")]
    public string? TickSize15 { get; set; }

    [CsvColumn("Upper Price Limit 15")]
    public string? UpperPriceLimit15 { get; set; }

    [CsvColumn("Tick Size 16")]
    public string? TickSize16 { get; set; }

    [CsvColumn("Upper Price Limit 16")]
    public int? UpperPriceLimit16 { get; set; }

    [CsvColumn("Tick Size 17")]
    public string? TickSize17 { get; set; }

    [CsvColumn("Upper Price Limit 17")]
    public int? UpperPriceLimit17 { get; set; }

    [CsvColumn("Tick Size 18")]
    public string? TickSize18 { get; set; }

    [CsvColumn("Upper Price Limit 18")]
    public int? UpperPriceLimit18 { get; set; }

    [CsvColumn("Tick Size 19")]
    public string? TickSize19 { get; set; }

    [CsvColumn("Upper Price Limit 19")]
    public string? UpperPriceLimit19 { get; set; }

    [CsvColumn("Tick Size 20")]
    public string? TickSize20 { get; set; }

    [CsvColumn("Upper Price Limit 20")]
    public string? UpperPriceLimit20 { get; set; }

    [CsvColumn("Number of Decimal Digits")]
    public int? NumberofDecimalDigits { get; set; }

    [CsvColumn("Unit of Quotation")]
    public string? UnitofQuotation { get; set; }

    [CsvColumn("Market Segment")]
    public string? MarketSegment { get; set; }

    [CsvColumn("Market Segment Supplement")]
    public string? MarketSegmentSupplement { get; set; }

    [CsvColumn("Clearing Location")]
    public string? ClearingLocation { get; set; }

    [CsvColumn("Primary Market MIC Code")]
    public string? PrimaryMarketMICCode { get; set; }

    [CsvColumn("Reporting Market")]
    public string? ReportingMarket { get; set; }

    [CsvColumn("Settlement Period")]
    public string? SettlementPeriod { get; set; }

    [CsvColumn("Settlement Currency")]
    public string? SettlementCurrency { get; set; }

    [CsvColumn("Closed Book Indicator")]
    public string? ClosedBookIndicator { get; set; }

    [CsvColumn("Market Imbalance Indicator")]
    public string? MarketImbalanceIndicator { get; set; }

    [CsvColumn("CUM/EX Indicator")]
    public string? CUMEXIndicator { get; set; }

    [CsvColumn("Minimum Iceberg Total Volume")]
    public string? MinimumIcebergTotalVolume { get; set; }

    [CsvColumn("Minimum Iceberg Display Volume")]
    public string? MinimumIcebergDisplayVolume { get; set; }

    [CsvColumn("EMDI Incremental A - Unnetted")]
    public string? EMDIIncrementalA_Unnetted { get; set; }

    [CsvColumn("EMDI Incremental A - Unnetted Port")]
    public string? EMDIIncrementalA_UnnettedPort { get; set; }

    [CsvColumn("EMDI Incremental B - Unnetted")]
    public string? EMDIIncrementalB_Unnetted { get; set; }

    [CsvColumn("EMDI Incremental B - Unnetted Port")]
    public string? EMDIIncrementalB_UnnettedPort { get; set; }

    [CsvColumn("EMDI Snapshot A - Unnetted")]
    public string? EMDISnapshotA_Unnetted { get; set; }

    [CsvColumn("EMDI Snapshot A - Unnetted Port")]
    public string? EMDISnapshotA_UnnettedPort { get; set; }

    [CsvColumn("EMDI Snapshot B - Unnetted")]
    public string? EMDISnapshotB_Unnetted { get; set; }

    [CsvColumn("EMDI Snapshot B - Unnetted Port")]
    public string? EMDISnapshotB_UnnettedPort { get; set; }

    [CsvColumn("EMDI Market Depth - Unnetted")]
    public string? EMDIMarketDepth_Unnetted { get; set; }

    [CsvColumn("EMDI Snapshot Recovery Time Interval - Unnetted")]
    public int EMDISnapshotRecoveryTimeInterval_Unnetted { get; set; }

    [CsvColumn("MDI Address A - Netted")]
    public string? MDIAddressA_Netted { get; set; }

    [CsvColumn("MDI Port A - Netted")]
    public string? MDIPortA_Netted { get; set; }

    [CsvColumn("MDI Address B - Netted")]
    public string? MDIAddressB_Netted { get; set; }

    [CsvColumn("MDI Port B - Netted")]
    public string? MDIPortB_Netted { get; set; }

    [CsvColumn("MDI Market Depth - Netted")]
    public string? MDIMarketDepth_Netted { get; set; }

    [CsvColumn("MDI Market Depth Time Interval - Netted")]
    public int MDIMarketDepthTimeInterval_Netted { get; set; }

    [CsvColumn("MDI Recovery Time Interval - Netted")]
    public int MDIRecoveryTimeInterval_Netted { get; set; }

    [CsvColumn("EOBI Incremental A")]
    public string? EOBIIncrementalA { get; set; }

    [CsvColumn("EOBI Incremental Port A")]
    public string? EOBIIncrementalPortA { get; set; }

    [CsvColumn("EOBI Incremental B")]
    public string? EOBIIncrementalB { get; set; }

    [CsvColumn("EOBI Incremental Port B")]
    public string? EOBIIncrementalPortB { get; set; }

    [CsvColumn("EOBI Snapshot A")]
    public string? EOBISnapshotA { get; set; }

    [CsvColumn("EOBI Snapshot Port A")]
    public string? EOBISnapshotPortA { get; set; }

    [CsvColumn("EOBI Snapshot B")]
    public string? EOBISnapshotB { get; set; }

    [CsvColumn("EOBI Snapshot Port B")]
    public string? EOBISnapshotPortB { get; set; }

    [CsvColumn("Market Maker Member ID")]
    public string? MarketMakerMemberID { get; set; }

    [CsvColumn("Market Maker")]
    public string? MarketMaker { get; set; }

    [CsvColumn("Regulatory Liquid Instrument")]
    public string? RegulatoryLiquidInstrument { get; set; }

    [CsvColumn("Pre-trade LIS Value")]
    public string? Pre_tradeLISValue { get; set; }

    [CsvColumn("Partition ID")]
    public string? PartitionID { get; set; }

    [CsvColumn("Multi CCP-eligible")]
    public string? MultiCCP_eligible { get; set; }

    [CsvColumn("Tick Size Band")]
    public string? TickSizeBand { get; set; }

    [CsvColumn("Security Sub Type")]
    public string? SecuritySubType { get; set; }

    [CsvColumn("Issue Date")]
    public string? IssueDate { get; set; }

    [CsvColumn("Underlying")]
    public string? Underlying { get; set; }

    [CsvColumn("Maturity Date")]
    public string? MaturityDate { get; set; }

    [CsvColumn("Flat Indicator")]
    public string? FlatIndicator { get; set; }

    [CsvColumn("Coupon Rate")]
    public string? CouponRate { get; set; }

    [CsvColumn("Previous Coupon Payment Date")]
    public string? PreviousCouponPaymentDate { get; set; }

    [CsvColumn("Next Coupon Payment Date")]
    public string? NextCouponPaymentDate { get; set; }

    [CsvColumn("Pool Factor")]
    public string? PoolFactor { get; set; }

    [CsvColumn("Indexation Coefficient")]
    public string? IndexationCoefficient { get; set; }

    [CsvColumn("Accrued Interest Calculation Method")]
    public string? AccruedInterestCalculationMethod { get; set; }

    [CsvColumn("Country Of Issue")]
    public string? CountryOfIssue { get; set; }

    [CsvColumn("Minimum Tradable Unit")]
    public string? MinimumTradableUnit { get; set; }

    [CsvColumn("In-Subscription")]
    public string? In_Subscription { get; set; }

    [CsvColumn("Strike Price")]
    public string? StrikePrice { get; set; }

    [CsvColumn("Minimum Order Quantity")]
    public string? MinimumOrderQuantity { get; set; }

    [CsvColumn("Off-Book Reporting Market")]
    public string? Off_BookReportingMarket { get; set; }

    [CsvColumn("Instrument Auction Type")]
    public string? InstrumentAuctionType { get; set; }

    [CsvColumn("Specialist Member ID")]
    public string? SpecialistMemberID { get; set; }

    [CsvColumn("Specialist")]
    public string? Specialist { get; set; }

    [CsvColumn("Liquidity Provider User Group")]
    public string? LiquidityProviderUserGroup { get; set; }

    [CsvColumn("Specialist User Group")]
    public string? SpecialistUserGroup { get; set; }

    [CsvColumn("Quoting Period Start")]
    public string? QuotingPeriodStart { get; set; }

    [CsvColumn("Quoting Period End")]
    public string? QuotingPeriodEnd { get; set; }

    [CsvColumn("Currency")]
    public string? Currency { get; set; }

    [CsvColumn("Warrant Type")]
    public string? WarrantType { get; set; }

    [CsvColumn("First Trading Date")]
    public string? FirstTradingDate { get; set; }

    [CsvColumn("Last Trading Date")]
    public string? LastTradingDate { get; set; }

    [CsvColumn("Deposit Type")]
    public string? DepositType { get; set; }

    [CsvColumn("Single Sided Quote Support")]
    public string? SingleSidedQuoteSupport { get; set; }

    [CsvColumn("Liquidity Class")]
    public string? LiquidityClass { get; set; }

    [CsvColumn("Cover Indicator")]
    public string? CoverIndicator { get; set; }

    [CsvColumn("VolatilityCorridorOpeningAuction")]
    public int? VolatilityCorridorOpeningAuction { get; set; }

    [CsvColumn("VolatilityCorridorIntradayAuction")]
    public int? VolatilityCorridorIntradayAuction { get; set; }

    [CsvColumn("VolatilityCorridorClosingAuction")]
    public int? VolatilityCorridorClosingAuction { get; set; }

    [CsvColumn("VolatilityCorridorContinuous")]
    public int? VolatilityCorridorContinuous { get; set; }

    [CsvColumn("DisableOnBookTrading")]
    public string? DisableOnBookTrading { get; set; }

    [CsvColumn("Maximum Order Quantity")]
    public string? MaximumOrderQuantity { get; set; }

    [CsvColumn("Maximum Order Value")]
    public string? MaximumOrderValue { get; set; }
}

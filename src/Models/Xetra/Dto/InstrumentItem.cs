﻿namespace Finance.Net.Models.Xetra.Dto;
public record InstrumentItem
{
    public string? ProductStatus { get; set; }
    public string? InstrumentStatus { get; set; }
    public string? Instrument { get; set; }
    public string? ISIN { get; set; }
    public string? ProductID { get; set; }
    public string? InstrumentID { get; set; }
    public string? WKN { get; set; }
    public string? Mnemonic { get; set; }
    public string? MICCode { get; set; }
    public string? CCPeligibleCode { get; set; }
    public string? TradingModelType { get; set; }
    public string? ProductAssignmentGroup { get; set; }
    public string? ProductAssignmentGroupDescription { get; set; }
    public string? DesignatedSponsorMemberID { get; set; }
    public string? DesignatedSponsor { get; set; }
    public string? PriceRangeValue { get; set; }
    public string? PriceRangePercentage { get; set; }
    public string? MinimumQuoteSize { get; set; }
    public string? InstrumentType { get; set; }
    public string? TickSize1 { get; set; }
    public string? UpperPriceLimitMax { get; set; }
    public double TickSize2 { get; set; }
    public double UpperPriceLimit2 { get; set; }
    public double TickSize3 { get; set; }
    public double UpperPriceLimit3 { get; set; }
    public string? TickSize4 { get; set; }
    public string? UpperPriceLimit4 { get; set; }
    public string? TickSize5 { get; set; }
    public string? UpperPriceLimit5 { get; set; }
    public string? TickSize6 { get; set; }
    public string? UpperPriceLimit6 { get; set; }
    public string? TickSize7 { get; set; }
    public string? UpperPriceLimit7 { get; set; }
    public string? TickSize8 { get; set; }
    public string? UpperPriceLimit8 { get; set; }
    public string? TickSize9 { get; set; }
    public string? UpperPriceLimit9 { get; set; }
    public string? TickSize10 { get; set; }
    public string? UpperPriceLimit10 { get; set; }
    public string? TickSize11 { get; set; }
    public string? UpperPriceLimit11 { get; set; }
    public string? TickSize12 { get; set; }
    public string? UpperPriceLimit12 { get; set; }
    public string? TickSize13 { get; set; }
    public string? UpperPriceLimit13 { get; set; }
    public string? TickSize14 { get; set; }
    public string? UpperPriceLimit14 { get; set; }
    public string? TickSize15 { get; set; }
    public string? UpperPriceLimit15 { get; set; }
    public string? TickSize16 { get; set; }
    public int? UpperPriceLimit16 { get; set; }
    public string? TickSize17 { get; set; }
    public int? UpperPriceLimit17 { get; set; }
    public string? TickSize18 { get; set; }
    public int? UpperPriceLimit18 { get; set; }
    public string? TickSize19 { get; set; }
    public string? UpperPriceLimit19 { get; set; }
    public string? TickSize20 { get; set; }
    public string? UpperPriceLimit20 { get; set; }
    public int? NumberofDecimalDigits { get; set; }
    public string? UnitofQuotation { get; set; }
    public string? MarketSegment { get; set; }
    public string? MarketSegmentSupplement { get; set; }
    public string? ClearingLocation { get; set; }
    public string? PrimaryMarketMICCode { get; set; }
    public string? ReportingMarket { get; set; }
    public string? SettlementPeriod { get; set; }
    public string? SettlementCurrency { get; set; }
    public string? ClosedBookIndicator { get; set; }
    public string? MarketImbalanceIndicator { get; set; }
    public string? CUMEXIndicator { get; set; }
    public string? MinimumIcebergTotalVolume { get; set; }
    public string? MinimumIcebergDisplayVolume { get; set; }
    public string? EMDIIncrementalA_Unnetted { get; set; }
    public string? EMDIIncrementalA_UnnettedPort { get; set; }
    public string? EMDIIncrementalB_Unnetted { get; set; }
    public string? EMDIIncrementalB_UnnettedPort { get; set; }
    public string? EMDISnapshotA_Unnetted { get; set; }
    public string? EMDISnapshotA_UnnettedPort { get; set; }
    public string? EMDISnapshotB_Unnetted { get; set; }
    public string? EMDISnapshotB_UnnettedPort { get; set; }
    public string? EMDIMarketDepth_Unnetted { get; set; }
    public int EMDISnapshotRecoveryTimeInterval_Unnetted { get; set; }
    public string? MDIAddressA_Netted { get; set; }
    public string? MDIPortA_Netted { get; set; }
    public string? MDIAddressB_Netted { get; set; }
    public string? MDIPortB_Netted { get; set; }
    public string? MDIMarketDepth_Netted { get; set; }
    public int MDIMarketDepthTimeInterval_Netted { get; set; }
    public int MDIRecoveryTimeInterval_Netted { get; set; }
    public string? EOBIIncrementalA { get; set; }
    public string? EOBIIncrementalPortA { get; set; }
    public string? EOBIIncrementalB { get; set; }
    public string? EOBIIncrementalPortB { get; set; }
    public string? EOBISnapshotA { get; set; }
    public string? EOBISnapshotPortA { get; set; }
    public string? EOBISnapshotB { get; set; }
    public string? EOBISnapshotPortB { get; set; }
    public string? MarketMakerMemberID { get; set; }
    public string? MarketMaker { get; set; }
    public string? RegulatoryLiquidInstrument { get; set; }
    public string? Pre_tradeLISValue { get; set; }
    public string? PartitionID { get; set; }
    public string? MultiCCP_eligible { get; set; }
    public string? TickSizeBand { get; set; }
    public string? SecuritySubType { get; set; }
    public string? IssueDate { get; set; }
    public string? Underlying { get; set; }
    public string? MaturityDate { get; set; }
    public string? FlatIndicator { get; set; }
    public string? CouponRate { get; set; }
    public string? PreviousCouponPaymentDate { get; set; }
    public string? NextCouponPaymentDate { get; set; }
    public string? PoolFactor { get; set; }
    public string? IndexationCoefficient { get; set; }
    public string? AccruedInterestCalculationMethod { get; set; }
    public string? CountryOfIssue { get; set; }
    public string? MinimumTradableUnit { get; set; }
    public string? In_Subscription { get; set; }
    public string? StrikePrice { get; set; }
    public string? MinimumOrderQuantity { get; set; }
    public string? Off_BookReportingMarket { get; set; }
    public string? InstrumentAuctionType { get; set; }
    public string? SpecialistMemberID { get; set; }
    public string? Specialist { get; set; }
    public string? LiquidityProviderUserGroup { get; set; }
    public string? SpecialistUserGroup { get; set; }
    public string? QuotingPeriodStart { get; set; }
    public string? QuotingPeriodEnd { get; set; }
    public string? Currency { get; set; }
    public string? WarrantType { get; set; }
    public string? FirstTradingDate { get; set; }
    public string? LastTradingDate { get; set; }
    public string? DepositType { get; set; }
    public string? SingleSidedQuoteSupport { get; set; }
    public string? LiquidityClass { get; set; }
    public string? CoverIndicator { get; set; }
    public int? VolatilityCorridorOpeningAuction { get; set; }
    public int? VolatilityCorridorIntradayAuction { get; set; }
    public int? VolatilityCorridorClosingAuction { get; set; }
    public int? VolatilityCorridorContinuous { get; set; }
    public string? DisableOnBookTrading { get; set; }
    public string? MaximumOrderQuantity { get; set; }
    public string? MaximumOrderValue { get; set; }
}
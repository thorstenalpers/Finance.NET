namespace Finance.Net.Models.Yahoo;

/// <summary>
/// Represents a financial report with various financial metrics.
/// </summary>
public record FinancialReport
{
    /// <summary>
    /// The company's stock symbol.
    /// </summary>
    public string? TickerSymbol { get; set; }

    /// <summary>
    /// Total revenue generated.
    /// </summary>
    public decimal? TotalRevenue { get; set; }

    /// <summary>
    /// Direct costs of goods/services sold.
    /// </summary>
    public decimal? CostOfRevenue { get; set; }

    /// <summary>
    /// Gross profit (Revenue - Cost of Revenue).
    /// </summary>
    public decimal? GrossProfit { get; set; }

    /// <summary>
    /// Operating expenses incurred.
    /// </summary>
    public decimal? OperatingExpense { get; set; }

    /// <summary>
    /// Operating income (Gross Profit - Operating Expenses).
    /// </summary>
    public decimal? OperatingIncome { get; set; }

    /// <summary>
    /// Net non-operating interest income/expense.
    /// </summary>
    public decimal? NetNonOperatingInterestIncomeExpense { get; set; }

    /// <summary>
    /// Other non-core income/expenses.
    /// </summary>
    public decimal? OtherIncomeExpense { get; set; }

    /// <summary>
    /// Pretax income before taxes.
    /// </summary>
    public decimal? PretaxIncome { get; set; }

    /// <summary>
    /// Income taxes provisioned.
    /// </summary>
    public decimal? TaxProvision { get; set; }

    /// <summary>
    /// Net income for common stockholders.
    /// </summary>
    public decimal? NetIncomeCommonStockholders { get; set; }

    /// <summary>
    /// Diluted net income for common stockholders.
    /// </summary>
    public decimal? DilutedNIAvailableToComStockholders { get; set; }

    /// <summary>
    /// Basic earnings per share.
    /// </summary>
    public decimal? BasicEPS { get; set; }

    /// <summary>
    /// Diluted earnings per share.
    /// </summary>
    public decimal? DilutedEPS { get; set; }

    /// <summary>
    /// Basic average shares for EPS.
    /// </summary>
    public decimal? BasicAverageShares { get; set; }

    /// <summary>
    /// Diluted average shares for EPS.
    /// </summary>
    public decimal? DilutedAverageShares { get; set; }

    /// <summary>
    /// Reported total operating income.
    /// </summary>
    public decimal? TotalOperatingIncomeAsReported { get; set; }

    /// <summary>
    /// Total expenses incurred.
    /// </summary>
    public decimal? TotalExpenses { get; set; }

    /// <summary>
    /// Net income from all operations.
    /// </summary>
    public decimal? NetIncomeFromContinuingAndDiscontinuedOperation { get; set; }

    /// <summary>
    /// Normalized income adjusted for irregularities.
    /// </summary>
    public decimal? NormalizedIncome { get; set; }

    /// <summary>
    /// Interest income earned.
    /// </summary>
    public decimal? InterestIncome { get; set; }

    /// <summary>
    /// Interest expense incurred.
    /// </summary>
    public decimal? InterestExpense { get; set; }

    /// <summary>
    /// Net interest income (Income - Expense).
    /// </summary>
    public decimal? NetInterestIncome { get; set; }

    /// <summary>
    /// EBIT: Earnings Before Interest and Taxes.
    /// </summary>
    public decimal? EBIT { get; set; }

    /// <summary>
    /// EBITDA: Earnings Before Interest, Taxes, Depreciation, and Amortization.
    /// </summary>
    public decimal? EBITDA { get; set; }

    /// <summary>
    /// Adjusted cost of revenue.
    /// </summary>
    public decimal? ReconciledCostOfRevenue { get; set; }

    /// <summary>
    /// Adjusted depreciation expense.
    /// </summary>
    public decimal? ReconciledDepreciation { get; set; }

    /// <summary>
    /// Net income from continuing operations.
    /// </summary>
    public decimal? NetIncomeFromContinuingOperationNetMinorityInterest { get; set; }

    /// <summary>
    /// Total unusual items, excluding goodwill.
    /// </summary>
    public decimal? TotalUnusualItemsExcludingGoodwill { get; set; }

    /// <summary>
    /// Total unusual items, including goodwill.
    /// </summary>
    public decimal? TotalUnusualItems { get; set; }

    /// <summary>
    /// Adjusted EBITDA for unusual items.
    /// </summary>
    public decimal? NormalizedEBITDA { get; set; }

    /// <summary>
    /// Tax rate used in calculations.
    /// </summary>
    public decimal? TaxRateForCalcs { get; set; }

    /// <summary>
    /// Tax effect of unusual items.
    /// </summary>
    public decimal? TaxEffectOfUnusualItems { get; set; }
}

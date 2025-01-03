![Banner](./src/banner.png)

[![.NET Standard 2.1](https://img.shields.io/badge/.NET%20Standard-2.1-blue)](#)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=thorstenalpers_Finance.NET&metric=alert_status)](https://sonarcloud.io/project/issues?issueStatuses=OPEN%2CCONFIRMED&id=thorstenalpers_Finance.NET)
[![CI Tests](https://github.com/thorstenalpers/Finance.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/thorstenalpers/Finance.NET/actions/workflows/ci.yml)
[![Coverage Status](https://coveralls.io/repos/github/thorstenalpers/Finance.NET/badge.svg?branch=develop)](https://coveralls.io/github/thorstenalpers/Finance.NET?branch=develop)
[![NuGet Version](https://img.shields.io/nuget/v/Finance.NET.svg)](https://www.nuget.org/packages/Finance.NET)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Finance.NET.svg)](https://www.nuget.org/packages/Finance.NET)
[![Star this repo](https://img.shields.io/github/stars/thorstenalpers/Finance.NET.svg?style=social&label=Star&maxAge=60)](https://github.com/thorstenalpers/Finance.NET)

An easy-to-use .NET library for accessing and aggregating financial data from multiple sources. This library provides functionality to retrieve financial information through APIs and HTML scraping from various providers.

## Features

* **Retrieve Instruments:** Access tradable ticker symbols and related details for financial instruments.
* **Fundamentals:** Fetch key financial metrics and company fundamentals.
* **Historical Records:** Obtain historical data for charting or analysis.
* **Real-Time Quotes:** Get live updates on stock prices and other market data.

## Table of contents

* [Getting started](#getting-started)
* [Services](#services)
  * [Yahoo! Finance](#yahoo-finance)
  * [Alpha Vantage](#alpha-vantage)
  * [DataHub](#datahub)
  * [Xetra](#xetra)
* [Disclaimer](#disclaimer)

## Getting started

### Installation

To integrate Finance .NET into your project, install it via NuGet.

```shell
dotnet add package Finance.NET
```

### Adding to Service Collection

Register Finance .NET in your service collection to enable dependency injection.

```csharp
services.AddFinanceNet();
```

You can also provide a custom configuration.

```csharp
services.AddFinanceNet(new FinanceNetConfiguration
{
    HttpTimeout = 5,
    HttpRetryCount = 3,                                
    AlphaVantageApiKey = "ALPHA_VANTAGE__API_KEY"
});
```

### Basic Usage

To fetch historical records and the latest ticker for Tesla stock.

```csharp
public async Task Run(IYahooFinanceService yahooService)
{
    var symbol = "TSLA";
    var startDate = new DateTime(2020, 1, 1);

    var records = await yahooService.GetRecordsAsync(symbol, startDate);
    foreach (var record in records)
    {
        Console.WriteLine($"Date={record.Date}: {record.Open} / {record.Close}");
    }

    var quote = await yahooService.GetQuoteAsync(symbol);
    Console.WriteLine($"Bid={quote.Bid}, Ask={quote.Ask}");
}
```

## Services

## Yahoo! Finance

[Yahoo! Finance](https://finance.yahoo.com/) is one of the most popular platforms for market data, company fundamentals, historical records, and real-time stock quotes.

<table>
  <thead>
    <tr>
      <th style="font-size: 16px; width: 1000px;">Methods</th>
    </tr>
  </thead>
  <tbody>
    <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetInstrumentsAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves a collection of financial instruments.

#### Parameters

* `EAssetType? filterByType`: An optional filter to specify the type of asset. If not provided, all asset types will be included.  
  Possible values for `EAssetType`:

  | Value  | Name   | Description                                    |
  |--------|--------|------------------------------------------------|
  | `1`    | Stock  | Most active Stocks                |
  | `2`    | ETF    | Most active ETFs      |
  | `3`    | Forex  | Available Currencies |
  | `4`    | Crypto | Available Cryptos        |
  | `5`    | Index  | Available World Indices         |

* `CancellationToken token`: (Optional) Cancellation token to cancel the operation if needed.

#### Returns

A task that resolves to an `IEnumerable<Instrument>` containing the following properties for each item:

| Property           | Type              | Description                            | Example         |
|--------------------|-------------------|----------------------------------------|-----------------|
| `Symbol`           | `string?`         | The ticker symbol of the instrument.   | AAPL            |
| `InstrumentType`   | `EAssetType?`     | The type of the financial instrument.  | Stock           |


#### Example

```csharp
public async Task Run(IYahooFinanceService yahooService)
{
    // Retrieve all instruments
    var instruments = await yahooService.GetInstrumentsAsync();

    // Retrieve only stock instruments
    var stockInstruments = await yahooService.GetInstrumentsAsync(EAssetType.Stock);

    foreach (var instrument in stockInstruments)
    {
        Console.WriteLine($"Symbol: {instrument.Symbol}, Type: {instrument.InstrumentType}");
    }
}
```
</details>
</td>
    </tr>
        <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetProfileAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>


#### Description

Retrieves the profile of a specific entity based on its symbol.

#### Parameters

* `string symbol`: The symbol of the quote (e.g., `"AAPL"` for Apple).
* `CancellationToken token`: (Optional) Cancellation token to cancel the operation if needed.

#### Returns

A task that resolves to a `Profile` containing the following properties:

| Property        | Type        | Description                                    | Example                     |
|-----------------|-------------|------------------------------------------------|-----------------------------|
| `Adress`        | `string?`   | The address of the entity.                     | One Apple Park Way, Cupertino, CA 95014 |
| `Phone`         | `string?`   | The phone number of the entity.                | +1-800-MY-APPLE             |
| `Website`       | `string?`   | The website URL of the entity.                 | https://www.apple.com       |
| `Sector`        | `string?`   | The sector in which the entity operates.       | Technology                  |
| `Industry`      | `string?`   | The industry the entity belongs to.            | Consumer Electronics        |
| `CntEmployees`  | `long?`     | The number of employees.                       | 164000                      |
| `Description`   | `string?`   | A brief description of the entity.             | Apple designs and ... |


#### Example

```csharp
public async Task Run(IYahooFinanceService yahooService)
{
    var profile = await yahooService.GetProfileAsync("AAPL");

    Console.WriteLine($"Address: {profile.Adress}");
    Console.WriteLine($"Sector: {profile.Sector}");
    Console.WriteLine($"Industry: {profile.Industry}");
    Console.WriteLine($"Description: {profile.Description}");
}
```

</details>
</td>
    </tr>
            <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetSummaryAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves the summary of a specific asset based on its symbol.

#### Parameters

* `string symbol`: The symbol of the quote (e.g., `"AAPL"` for Apple).
* `CancellationToken token`: (Optional) Cancellation token to cancel the operation if needed.

#### Returns

A task that resolves to a `Summary` containing the following properties:

| Property                | Type          | Description                                                   | Example                 |
|-------------------------|---------------|---------------------------------------------------------------|-------------------------|
| `Name`                  | `string?`     | Name of the asset.                                            | Apple Inc.              |
| `MarketTimeNotice`      | `string?`     | Notice of market status.                                      | Market Closed           |
| `PreviousClose`         | `decimal?`    | Previous closing price.                                       | 180.14                  |
| `Open`                  | `decimal?`    | Opening price of the stock.                                   | 182.20                  |
| `Bid`                   | `decimal?`    | Current bid price.                                            | 180.00                  |
| `Ask`                   | `decimal?`    | Current ask price.                                            | 181.00                  |
| `DaysRange_Min`         | `decimal?`    | Minimum price today.                                          | 179.50                  |
| `DaysRange_Max`         | `decimal?`    | Maximum price today.                                          | 183.00                  |
| `WeekRange52_Min`       | `decimal?`    | Minimum price in 52 weeks.                                    | 130.20                  |
| `WeekRange52_Max`       | `decimal?`    | Maximum price in 52 weeks.                                    | 190.50                  |
| `Volume`                | `decimal?`    | Total volume traded today.                                    | 25,000,000              |
| `AvgVolume`             | `decimal?`    | Average daily volume.                                         | 30,000,000              |
| `MarketCap_Intraday`    | `decimal?`    | Market cap in the current session.                            | 2.85T                   |
| `Beta_5Y_Monthly`       | `decimal?`    | 5-year beta (monthly data).                                   | 1.20                    |
| `PE_Ratio_TTM`          | `decimal?`    | Price-to-earnings ratio (TTM).                                | 28.90                   |
| `EPS_TTM`               | `decimal?`    | Earnings per share (TTM).                                     | 6.22                    |
| `EarningsDate`          | `DateTime?`   | Date of the next earnings report.                             | 2025-02-15              |
| `Forward_Dividend`      | `decimal?`    | Expected forward dividend.                                    | 0.88                    |
| `Forward_Yield`         | `decimal?`    | Forward dividend yield.                                       | 0.49%                   |
| `Ex_DividendDate`       | `DateTime?`   | Ex-dividend date.                                             | 2025-01-10              |
| `OneYearTargetEst`      | `decimal?`    | One-year target price estimate. 

#### Example

```csharp
public async Task Run(IYahooFinanceService yahooService)
{
    // Retrieve the summary for Apple Inc.
    var summary = await yahooService.GetSummaryAsync("AAPL");

    Console.WriteLine($"Name: {summary.Name}");
    Console.WriteLine($"Previous Close: {summary.PreviousClose}");
    Console.WriteLine($"Open: {summary.Open}");
    Console.WriteLine($"Bid: {summary.Bid}");
    Console.WriteLine($"Ask: {summary.Ask}");
    Console.WriteLine($"Average Volume: {summary.AvgVolume}");
    Console.WriteLine($"EPS (TTM): {summary.EPS_TTM}");
}
```

</details>
</td>
    </tr>
            <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetFinancialsAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves the financial reports for a specified asset identified by its symbol.

#### Parameters

* `string symbol`: The symbol of the quote (e.g., `"AAPL"` for Apple).
* `CancellationToken token`: (Optional) Cancellation token to cancel the operation if needed.

#### Returns

A task that resolves to a `Dictionary<string, FinancialReport>` where the key is the label (e.g., "Annual Report 2024") and the value is a `FinancialReport` containing the following properties:

| Property                                         | Type          | Description                                                                                 | Example                  |
|--------------------------------------------------|---------------|---------------------------------------------------------------------------------------------|--------------------------|
| `TickerSymbol`                                   | `string?`     | The company's stock symbol.                                                                | AAPL                     |
| `TotalRevenue`                                   | `decimal?`    | Total revenue generated.                                                                   | 394,328,000,000          |
| `CostOfRevenue`                                  | `decimal?`    | Direct costs of goods/services sold.                                                       | 213,459,000,000          |
| `GrossProfit`                                    | `decimal?`    | Gross profit (Revenue - Cost of Revenue).                                                  | 180,869,000,000          |
| `OperatingExpense`                               | `decimal?`    | Operating expenses incurred.                                                               | 34,152,000,000           |
| `OperatingIncome`                                | `decimal?`    | Operating income (Gross Profit - Operating Expenses).                                      | 146,717,000,000          |
| `NetNonOperatingInterestIncomeExpense`           | `decimal?`    | Net non-operating interest income/expense.                                                 | 2,500,000,000            |
| `OtherIncomeExpense`                             | `decimal?`    | Other non-core income/expenses.                                                            | -1,200,000,000           |
| `PretaxIncome`                                   | `decimal?`    | Pretax income before taxes.                                                                | 148,017,000,000          |
| `TaxProvision`                                   | `decimal?`    | Income taxes provisioned.                                                                  | 25,000,000,000           |
| `NetIncomeCommonStockholders`                    | `decimal?`    | Net income for common stockholders.                                                       | 123,017,000,000          |
| `DilutedNIAvailableToComStockholders`            | `decimal?`    | Diluted net income for common stockholders.                                               | 120,517,000,000          |
| `BasicEPS`                                       | `decimal?`    | Basic earnings per share.                                                                  | 6.25                     |
| `DilutedEPS`                                     | `decimal?`    | Diluted earnings per share.                                                                | 6.15                     |
| `BasicAverageShares`                             | `decimal?`    | Basic average shares for EPS.                                                             | 19,700,000,000           |
| `DilutedAverageShares`                           | `decimal?`    | Diluted average shares for EPS.                                                           | 19,600,000,000           |
| `TotalOperatingIncomeAsReported`                 | `decimal?`    | Reported total operating income.                                                          | 146,700,000,000          |
| `TotalExpenses`                                  | `decimal?`    | Total expenses incurred.                                                                   | 247,611,000,000          |
| `NetIncomeFromContinuingAndDiscontinuedOperation`| `decimal?`    | Net income from all operations.                                                           | 123,017,000,000          |
| `NormalizedIncome`                               | `decimal?`    | Normalized income adjusted for irregularities.                                             | 125,500,000,000          |
| `InterestIncome`                                 | `decimal?`    | Interest income earned.                                                                   | 5,000,000,000            |
| `InterestExpense`                                | `decimal?`    | Interest expense incurred.                                                                | 2,500,000,000            |
| `NetInterestIncome`                              | `decimal?`    | Net interest income (Income - Expense).                                                  | 2,500,000,000            |
| `EBIT`                                           | `decimal?`    | Earnings Before Interest and Taxes.                                                       | 148,217,000,000          |
| `EBITDA`                                         | `decimal?`    | Earnings Before Interest, Taxes, Depreciation, and Amortization.                          | 151,217,000,000          |
| `ReconciledCostOfRevenue`                        | `decimal?`    | Adjusted cost of revenue.                                                                 | 212,000,000,000          |
| `ReconciledDepreciation`                         | `decimal?`    | Adjusted depreciation expense.                                                            | 3,000,000,000            |
| `NetIncomeFromContinuingOperationNetMinorityInterest` | `decimal?`| Net income from continuing operations.                                                   | 121,017,000,000          |
| `TotalUnusualItemsExcludingGoodwill`             | `decimal?`    | Total unusual items, excluding goodwill.                                                  | -2,000,000,000           |
| `TotalUnusualItems`                              | `decimal?`    | Total unusual items, including goodwill.                                                  | -2,000,000,000           |
| `NormalizedEBITDA`                               | `decimal?`    | Adjusted EBITDA for unusual items.                                                       | 153,217,000,000          |
| `TaxRateForCalcs`                                | `decimal?`    | Tax rate used in calculations.                                                           | 16.9%                    |
| `TaxEffectOfUnusualItems`                        | `decimal?`    | Tax effect of unusual items.                                                             | -500,000,000             |


#### Example

```csharp
public async Task Run(IYahooFinanceService yahooService)
{
    // Retrieve financial reports for Apple Inc.
    var financialReports = await yahooService.GetFinancialsAsync("AAPL");

    foreach (var label in financialReports.Keys)
    {
        var report = financialReports[label];
        Console.WriteLine($"Label: {label}");
        Console.WriteLine($"Ticker Symbol: {report.TickerSymbol}");
        Console.WriteLine($"Total Revenue: {report.TotalRevenue}");
        Console.WriteLine($"Cost of Revenue: {report.CostOfRevenue}");
        Console.WriteLine($"Gross Profit: {report.GrossProfit}");
        Console.WriteLine($"Operating Income: {report.OperatingIncome}");
        Console.WriteLine($"Net Income: {report.NetIncomeCommonStockholders}");
        Console.WriteLine();
    }
}
```

</details>
</td>
    </tr>
        <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetRecordsAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves historical stock market data records for a specified asset identified by its symbol. Users can specify an optional date range.

#### Parameters

* `string symbol`: The symbol of the quote (e.g., `"AAPL"` for Apple).
* `DateTime? startDate`: (Optional) Start date for retrieving historical records. Defaults to 7 days before the current date if not provided.
* `DateTime? endDate`: (Optional) End date for retrieving historical records. Defaults to the current date if not provided.
* `CancellationToken token`: (Optional) Cancellation token to cancel the operation if needed.

#### Returns

A task that resolves to an `IEnumerable<Record>`, where each `Record` represents a historical data point with the following properties:

| Property          | Type        | Description                                                                                 | Example           |
|-------------------|-------------|---------------------------------------------------------------------------------------------|-------------------|
| `Date`            | `DateTime`  | The date of the record.                                                                     | 2025-01-01        |
| `Open`            | `decimal?`  | The opening price.                                                                          | 150.25            |
| `High`            | `decimal?`  | The highest price during the trading session.                                               | 155.00            |
| `Low`             | `decimal?`  | The lowest price during the trading session.                                                | 148.50            |
| `Close`           | `decimal?`  | The closing price at the end of the trading session.                                        | 152.75            |
| `AdjustedClose`   | `decimal?`  | The adjusted closing price, accounting for stock splits and dividends.                      | 153.00            |
| `Volume`          | `long?`     | The trading volume (number of shares traded).                                               | 10,000,000        |

#### Example

```csharp
public async Task Run(IYahooFinanceService yahooService)
{
    // Retrieve historical records for Apple Inc. for the last 30 days
    var startDate = DateTime.UtcNow.AddDays(-30);
    var endDate = DateTime.UtcNow;

    var records = await yahooService.GetRecordsAsync("AAPL", startDate, endDate);

    foreach (var record in records)
    {
        Console.WriteLine($"Date: {record.Date:yyyy-MM-dd}");
        Console.WriteLine($"Open: {record.Open:C}");
        Console.WriteLine($"Close: {record.Close:C}");
        Console.WriteLine();
    }
}
```

</details>
</td>
    </tr>
        <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetQuoteAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves detailed information about a specific financial quote, identified by its symbol. This API is useful for accessing comprehensive data about a stock, ETF, or other traded financial instruments.

#### Parameters

- `string symbol`: The symbol of the quote (e.g., `"AAPL"` for Apple).
- `CancellationToken token`: (Optional) A cancellation token that can be used to cancel the operation if needed.

#### Returns

A task that resolves to a `Quote` object. The `Quote` record contains detailed information about the requested financial instrument, as described in the table below.

| Property                          | Type         | Description                                                             | Example           |
|------------------------------------|--------------|-------------------------------------------------------------------------|-------------------|
| `Language`                         | `string?`    | The language of the quote.                                               | "en"              |
| `Region`                           | `string?`    | The region of the quote.                                                 | "US"              |
| `QuoteType`                        | `string?`    | The type of the quote.                                                   | "equity"          |
| `TypeDisp`                         | `string?`    | The display type of the quote.                                           | "STOCK"           |
| `QuoteSourceName`                  | `string?`    | The source of the quote.                                                 | "Yahoo Finance"   |
| `CustomPriceAlertConfidence`       | `string?`    | The confidence level of a custom price alert.                            | "HIGH"            |
| `Currency`                         | `string?`    | The currency in which the stock is traded.                               | "USD"             |
| `Exchange`                         | `string?`    | The exchange on which the stock is listed.                               | "NASDAQ"          |
| `ShortName`                        | `string?`    | The short name of the symbol.                                            | "AAPL"            |
| `LongName`                         | `string?`    | The full name of the symbol.                                             | "Apple Inc."      |
| `ExchangeTimezoneName`             | `string?`    | The time zone of the exchange.                                           | "America/New_York"|
| `ExchangeTimezoneShortName`        | `string?`    | The abbreviated time zone of the exchange.                               | "EST"             |
| `GmtOffSetMilliseconds`            | `long?`      | The GMT offset in milliseconds.             | -18000000         |
| `Market`                           | `string?`    | The market the instrument is listed on.                                  | "Equity"          |
| `EsgPopulated`                     | `bool?`      | Indicates if ESG.  | `true`            |
| `RegularMarketChangePercent`       | `double?`    | The percentage change in the regular market price.                       | 2.35              |
| `RegularMarketPrice`               | `double?`    | The regular market price of the stock.                                   | 145.67            |
| `MarketState`                      | `string?`    | The market state (e.g., open or closed).                                 | "OPEN"            |
| `FullExchangeName`                 | `string?`    | The full name of the exchange.                                           | "NASDAQ Stock Market"|
| `FinancialCurrency`                | `string?`    | The financial currency used for the quote.                               | "USD"             |
| `RegularMarketOpen`                | `double?`    | The opening price of the regular market.                                 | 143.50            |
| `AverageDailyVolume3Month`         | `long?`      | The average volume over the last 3 months.                               | 1500000           |
| `AverageDailyVolume10Day`          | `long?`      | The average volume over the last 10 days.                                | 2000000           |
| `FiftyTwoWeekLowChange`            | `double?`    | The change in the 52-week low price.                                    | 10.00             |
| `FiftyTwoWeekLowChangePercent`     | `double?`    | The percentage change in the 52-week low price.                         | 7.5               |
| `FiftyTwoWeekRange`                | `string?`    | The 52-week price range.                                                 | "120.00 - 160.00" |
| `FiftyTwoWeekHighChange`           | `double?`    | The change in the 52-week high price.                                   | -5.00             |
| `FiftyTwoWeekHighChangePercent`    | `double?`    | The percentage change in the 52-week high price.                        | -3.12             |
| `FiftyTwoWeekLow`                  | `double?`    | The price at its 52-week low.                                           | 120.00            |
| `FiftyTwoWeekHigh`                 | `double?`    | The price at its 52-week high.                                          | 160.00            |
| `FiftyTwoWeekChangePercent`        | `double?`    | The percentage change in the 52-week price.                             | 5.0               |
| `EarningsDate`                     | `DateTime?`  | The earnings date.                                                       | 2025-02-01        |
| `DividendRate`                     | `double?`    | The current dividend rate.                                               | 0.22              |
| `DividendDate`                     | `DateTime?`  | The date of the next dividend payment.                                   | 2025-04-15        |
| `TrailingAnnualDividendYield`      | `double?`    | The trailing annual dividend yield.                                      | 1.5               |
| `MarketCap`                        | `long?`      | The market capitalization of the company.                                | 2450000000000     |
| `ForwardPe`                        | `double?`    | The forward PE ratio.                                                    | 28.9              |
| `PriceToBook`                      | `double?`    | The price-to-book ratio.                                                 | 12.5              |
| `AverageAnalystRating`             | `string?`    | The average analyst rating.                                              | "Buy"             |
| `Tradeable`                        | `bool?`      | Indicates whether the instrument is tradeable.                           | `true`            |
| `HasPrePostMarketData`             | `bool?`      | Has the quote pre/post-market data.                    | `true`            |
| `FirstTradeDate`                   | `DateTime?`  | The date of the first trade.                                             | 1980-12-12        |
| `DisplayName`                      | `string?`    | The display name of the stock.                                           | "Apple Inc."      |
| `Symbol`                           | `string?`    | The symbol (ticker) of the stock.                                        | "AAPL"            |


#### Example

```csharp
public async Task DisplayQuote(IYahooFinanceService yahooService)
{
    // Retrieve a quote for Apple Inc.
    var quote = await yahooService.GetQuoteAsync("AAPL");

    Console.WriteLine($"Symbol: {quote.Symbol}");
    Console.WriteLine($"Name: {quote.ShortName}");
    Console.WriteLine($"Market Price: {quote.RegularMarketPrice:C}");
    Console.WriteLine($"52-Week High: {quote.FiftyTwoWeekHigh:C}");
    Console.WriteLine($"52-Week Low: {quote.FiftyTwoWeekLow:C}");
    Console.WriteLine($"Market Cap: {quote.MarketCap:N0}");
    Console.WriteLine($"Currency: {quote.Currency}");
}
```

</details>
</td>
    </tr>
            <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetQuotesAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves quote data for multiple financial instruments identified by their symbols. The data includes detailed information about each instrument, such as pricing, market performance, and other financial metrics.

#### Parameters

* `List<string> symbols`: A list of symbols for which to retrieve data (e.g., `["AAPL", "MSFT", "GOOGL"]`).
* `CancellationToken token`: (Optional) Cancellation token to cancel the operation if needed.

#### Returns

A task that resolves to an `IEnumerable<Quote>`, where each `Quote` provides comprehensive data about a specific instrument.

| Property                          | Type         | Description                                                             | Example           |
|------------------------------------|--------------|-------------------------------------------------------------------------|-------------------|
| `Language`                         | `string?`    | The language of the quote.                                               | "en"              |
| `Region`                           | `string?`    | The region of the quote.                                                 | "US"              |
| `QuoteType`                        | `string?`    | The type of the quote.                                                   | "equity"          |
| `TypeDisp`                         | `string?`    | The display type of the quote.                                           | "STOCK"           |
| `QuoteSourceName`                  | `string?`    | The source of the quote.                                                 | "Yahoo Finance"   |
| `CustomPriceAlertConfidence`       | `string?`    | The confidence level of a custom price alert.                            | "HIGH"            |
| `Currency`                         | `string?`    | The currency in which the stock is traded.                               | "USD"             |
| `Exchange`                         | `string?`    | The exchange on which the stock is listed.                               | "NASDAQ"          |
| `ShortName`                        | `string?`    | The short name of the symbol.                                            | "AAPL"            |
| `LongName`                         | `string?`    | The full name of the symbol.                                             | "Apple Inc."      |
| `ExchangeTimezoneName`             | `string?`    | The time zone of the exchange.                                           | "America/New_York"|
| `ExchangeTimezoneShortName`        | `string?`    | The abbreviated time zone of the exchange.                               | "EST"             |
| `GmtOffSetMilliseconds`            | `long?`      | The GMT offset in milliseconds.             | -18000000         |
| `Market`                           | `string?`    | The market the instrument is listed on.                                  | "Equity"          |
| `EsgPopulated`                     | `bool?`      | Indicates if ESG.  | `true`            |
| `RegularMarketChangePercent`       | `double?`    | The percentage change in the regular market price.                       | 2.35              |
| `RegularMarketPrice`               | `double?`    | The regular market price of the stock.                                   | 145.67            |
| `MarketState`                      | `string?`    | The market state (e.g., open or closed).                                 | "OPEN"            |
| `FullExchangeName`                 | `string?`    | The full name of the exchange.                                           | "NASDAQ Stock Market"|
| `FinancialCurrency`                | `string?`    | The financial currency used for the quote.                               | "USD"             |
| `RegularMarketOpen`                | `double?`    | The opening price of the regular market.                                 | 143.50            |
| `AverageDailyVolume3Month`         | `long?`      | The average volume over the last 3 months.                               | 1500000           |
| `AverageDailyVolume10Day`          | `long?`      | The average volume over the last 10 days.                                | 2000000           |
| `FiftyTwoWeekLowChange`            | `double?`    | The change in the 52-week low price.                                    | 10.00             |
| `FiftyTwoWeekLowChangePercent`     | `double?`    | The percentage change in the 52-week low price.                         | 7.5               |
| `FiftyTwoWeekRange`                | `string?`    | The 52-week price range.                                                 | "120.00 - 160.00" |
| `FiftyTwoWeekHighChange`           | `double?`    | The change in the 52-week high price.                                   | -5.00             |
| `FiftyTwoWeekHighChangePercent`    | `double?`    | The percentage change in the 52-week high price.                        | -3.12             |
| `FiftyTwoWeekLow`                  | `double?`    | The price at its 52-week low.                                           | 120.00            |
| `FiftyTwoWeekHigh`                 | `double?`    | The price at its 52-week high.                                          | 160.00            |
| `FiftyTwoWeekChangePercent`        | `double?`    | The percentage change in the 52-week price.                             | 5.0               |
| `EarningsDate`                     | `DateTime?`  | The earnings date.                                                       | 2025-02-01        |
| `DividendRate`                     | `double?`    | The current dividend rate.                                               | 0.22              |
| `DividendDate`                     | `DateTime?`  | The date of the next dividend payment.                                   | 2025-04-15        |
| `TrailingAnnualDividendYield`      | `double?`    | The trailing annual dividend yield.                                      | 1.5               |
| `MarketCap`                        | `long?`      | The market capitalization of the company.                                | 2450000000000     |
| `ForwardPe`                        | `double?`    | The forward PE ratio.                                                    | 28.9              |
| `PriceToBook`                      | `double?`    | The price-to-book ratio.                                                 | 12.5              |
| `AverageAnalystRating`             | `string?`    | The average analyst rating.                                              | "Buy"             |
| `Tradeable`                        | `bool?`      | Indicates whether the instrument is tradeable.                           | `true`            |
| `HasPrePostMarketData`             | `bool?`      | Has the quote pre/post-market data.                    | `true`            |
| `FirstTradeDate`                   | `DateTime?`  | The date of the first trade.                                             | 1980-12-12        |
| `DisplayName`                      | `string?`    | The display name of the stock.                                           | "Apple Inc."      |
| `Symbol`                           | `string?`    | The symbol (ticker) of the stock.                                        | "AAPL"            |

#### Example

```csharp
public async Task Run(IYahooFinanceService yahooService)
{
    // Retrieve quotes for Apple, Microsoft, and Google
    var symbols = new List<string> { "AAPL", "MSFT", "GOOGL" };

    var quotes = await yahooService.GetQuotesAsync(symbols);

    foreach (var quote in quotes)
    {
        Console.WriteLine($"Symbol: {quote.Symbol}");
        Console.WriteLine($"Name: {quote.DisplayName}");
        Console.WriteLine($"Price: {quote.RegularMarketPrice:C}");
        Console.WriteLine($"52-Week High: {quote.FiftyTwoWeekHigh:C}");
        Console.WriteLine($"52-Week Low: {quote.FiftyTwoWeekLow:C}");
        Console.WriteLine($"Market Cap: {quote.MarketCap:N0}");
        Console.WriteLine($"Dividend Yield: {quote.DividendYield:P}");
        Console.WriteLine($"Earnings Date: {quote.EarningsDate:yyyy-MM-dd}");
        Console.WriteLine();
    }
}
```

</details>
</td>
    </tr>
      </tbody>
</table>

## Alpha Vantage

[Alpha Vantage](https://www.alphavantage.co) is a well-regarded provider of stock, forex, and cryptocurrency data, offering historical records and intraday prices.

#### Get an API key

To get started, obtain a free API key from [Alpha Vantage](https://www.alphavantage.co/support/#api-key).

#### Configure API key

After acquiring your API key, configure it in your service collection:

```csharp
services.AddFinanceNet(new FinanceNetConfiguration
{
    AlphaVantageApiKey = "API_KEY"
});
```

<table>
  <thead>
    <tr>
      <th style="font-size: 16px; width: 1000px;">Methods</th>
    </tr>
  </thead>
  <tbody>
    <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetOverviewAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves an instrument overview for a specified stock symbol.

#### Parameters

* `string symbol`: The symbol of the asset (e.g., `"AAPL"` for Apple).
* `CancellationToken token`: (Optional) A token to cancel the operation if needed.

#### Returns

A task that resolves to an `InstrumentOverview?`. The `InstrumentOverview` contains the following properties that provide key information about the company:

| Property                      | Type       | Description                                                                                | Example                           |
|-------------------------------|------------|--------------------------------------------------------------------------------------------|-----------------------------------|
| `Symbol`                       | `string?`  | The stock symbol.                                                                          | "AAPL"                            |
| `AssetType`                    | `string?`  | The type of asset (e.g., stock, ETF).                                                      | "Equity"                          |
| `Name`                         | `string?`  | The name of the ticker or company.                                                         | "Apple Inc."                      |
| `Description`                  | `string?`  | A brief company description.                                                               | "Designs ... ."   |
| `CIK`                          | `string?`  | The Central Index Key (CIK) of the company.                                                | "0000320193"                      |
| `Exchange`                     | `string?`  | The exchange where the company is listed.                                                  | "NASDAQ"                          |
| `Currency`                     | `string?`  | The currency used for financials.                                                          | "USD"                             |
| `Country`                      | `string?`  | The country where the company is located.                                                  | "United States"                   |
| `Sector`                       | `string?`  | The company's sector (e.g., Technology).                                                   | "Technology"                      |
| `Industry`                     | `string?`  | The industry the company operates in.                                                      | "Consumer Electronics"            |
| `Address`                      | `string?`  | The company's headquarters address.                                                        | "Cupertino, CA"                   |
| `OfficialSite`                 | `string?`  | The official website of the company.                                                       | "https://www.apple.com"            |
| `FiscalYearEnd`                | `string?`  | The fiscal year end date.                                                                   | "September 30"                    |
| `LatestQuarter`                | `string?`  | The most recent available quarter.                                                         | "Q3 2024"                         |
| `MarketCapitalization`         | `long?`    | The market capitalization.                                                                  | 2320000000000                     |
| `EBITDA`                       | `string?`  | EBITDA.                           | "11200000000"                     |
| `PERatio`                      | `string?`  | The Price-to-Earnings ratio.                                                                | "27.5"                            |
| `PEGRatio`                     | `string?`  | The Price/Earnings-to-Growth ratio.                                                         | "1.4"                             |
| `BookValue`                    | `string?`  | The company's book value.                                                                  | "10.52"                           |
| `DividendPerShare`             | `string?`  | The dividend per share.                                                                     | "0.82"                            |
| `DividendYield`                | `string?`  | The dividend yield.                                                                         | "1.5%"                             |
| `EPS`                          | `string?`  | Earnings per share.                                                                         | "5.26"                            |
| `RevenuePerShareTTM`           | `string?`  | Revenue per share for the trailing twelve months.                                           | "30.5"                            |
| `ProfitMargin`                 | `string?`  | Profit margin.                                                                              | "25%"                             |
| `OperatingMarginTTM`           | `string?`  | Operating margin for the trailing twelve months.                                            | "22%"                             |
| `ReturnOnAssetsTTM`            | `string?`  | Return on assets for the trailing twelve months.                                           | "14%"                             |
| `ReturnOnEquityTTM`            | `string?`  | Return on equity for the trailing twelve months.                                           | "40%"                             |
| `RevenueTTM`                   | `string?`  | Revenue for the trailing twelve months.                                                     | "386000000000"                    |
| `GrossProfitTTM`               | `string?`  | Gross profit for the trailing twelve months.                                                | "160000000000"                    |
| `DilutedEPSTTM`                | `string?`  | Diluted earnings per share for the trailing twelve months.                                 | "5.10"                            |
| `QuarterlyEarningsGrowthYOY`   | `string?`  | Quarterly earnings growth year-over-year.                                                  | "15%"                             |
| `QuarterlyRevenueGrowthYOY`    | `string?`  | Quarterly revenue growth year-over-year.                                                   | "10%"                             |
| `AnalystTargetPrice`           | `string?`  | Analyst target price for the stock.                                                        | "175.00"                          |
| `AnalystRatingStrongBuy`       | `string?`  | Percentage of analysts recommending a strong buy.                                           | "60%"                             |
| `AnalystRatingBuy`             | `string?`  | Percentage of analysts recommending a buy.                                                 | "30%"                             |
| `AnalystRatingHold`            | `string?`  | Percentage of analysts recommending a hold.                                                | "10%"                             |
| `AnalystRatingSell`            | `string?`  | Percentage of analysts recommending a sell.                                                 | "0%"                              |
| `AnalystRatingStrongSell`      | `string?`  | Percentage of analysts recommending a strong sell.                                          | "0%"                              |
| `TrailingPE`                   | `string?`  | Trailing Price-to-Earnings ratio.                                                           | "28"                              |
| `ForwardPE`                    | `string?`  | Forward Price-to-Earnings ratio.                                                            | "25"                              |
| `PriceToSalesRatioTTM`         | `string?`  | Price-to-Sales ratio for the trailing twelve months.                                        | "6.5"                             |
| `PriceToBookRatio`             | `string?`  | Price-to-Book ratio.                                                                        | "4.3"                             |
| `EVToRevenue`                  | `string?`  | Enterprise value-to-revenue ratio.                                                          | "8.2"                             |
| `EVToEBITDA`                   | `string?`  | Enterprise value-to-EBITDA ratio.                                                           | "14.5"                            |
| `Beta`                         | `string?`  | Beta value, measuring stock volatility.                                                     | "1.2"                             |
| `FiftySecondWeekHigh`          | `string?`  | 52-week high stock price.                                                                   | "179.50"                          |
| `FiftySecondWeekLow`           | `string?`  | 52-week low stock price.                                                                    | "120.10"                          |
| `FiftyDayMovingAverage`        | `string?`  | 50-day moving average.                                                                      | "153.25"                          |
| `TwoHundredDayMovingAverage`   | `string?`  | 200-day moving average.                                                                     | "157.80"                          |
| `SharesOutstanding`            | `string?`  | Number of shares outstanding.                                                               | "5000000000"                      |
| `DividendDate`                 | `string?`  | Next dividend payment date.                                                                  | "2025-02-01"                      |
| `ExDividendDate`               | `string?`  | Ex-dividend date. 


#### Example

```csharp
public async Task Run(IAlphaVantageService alphaVantageService)
{
    // Retrieve the overview for Apple Inc.
    var overview = await alphaVantageService.GetOverviewAsync("AAPL");

    if (overview != null)
    {
        Console.WriteLine($"Symbol: {overview.Symbol}");
        Console.WriteLine($"Name: {overview.Name}");
        Console.WriteLine($"Sector: {overview.Sector}");
        Console.WriteLine($"Market Capitalization: {overview.MarketCapitalization}");
        Console.WriteLine($"Dividend Yield: {overview.DividendYield}");
        Console.WriteLine($"P/E Ratio: {overview.PERatio}");
        Console.WriteLine($"Revenue (TTM): {overview.RevenueTTM}");
    }
}
```

</details>
</td>
    </tr>
        <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetRecordsAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves historical daily stock records for a given symbol within an optional date range.

#### Parameters

* `string symbol`: The stock symbol (e.g., `"AAPL"` for Apple).
* `DateTime? startDate`: (Optional) Start date for the records. Defaults to 7 days ago.
* `DateTime? endDate`: (Optional) End date for the records. Defaults to current date.
* `CancellationToken token`: (Optional) A token to cancel the operation.

#### Returns

A task that resolves to an `IEnumerable<Record>`, with the following properties:

| Property            | Type       | Description                                                                              | Example     |
|---------------------|------------|------------------------------------------------------------------------------------------|-------------|
| `Date`              | `DateTime` | The date of the record.                                                                  | "2024-12-15"|
| `Open`              | `double?`  | The opening price of the asset.                                                          | 150.25      |
| `Low`               | `double?`  | The lowest price of the asset on that date.                                               | 148.75      |
| `High`              | `double?`  | The highest price of the asset on that date.                                             | 153.50      |
| `Close`             | `double?`  | The closing price of the asset.                                                          | 151.00      |
| `AdjustedClose`     | `double?`  | The adjusted closing price, considering stock splits and dividends.                       | 150.80      |
| `Volume`            | `long?`    | The trading volume of the asset on that date.                                            | 1000000     |
| `SplitCoefficient`  | `double?`  | The stock split coefficient, if any, for the given date.                                  | 1.0         |

#### Example

```csharp
public async Task Run(IAlphaVantageService alphaVantageService)
{
    // Retrieve historical records for Apple Inc. (AAPL)
    var records = await alphaVantageService.GetRecordsAsync("AAPL", DateTime.Now.AddDays(-7), DateTime.Now);

    foreach (var record in records)
    {
        Console.WriteLine($"Date: {record.Date.ToShortDateString()}");
        Console.WriteLine($"Open: {record.Open}");
        Console.WriteLine($"High: {record.High}");
        Console.WriteLine($"Low: {record.Low}");
        Console.WriteLine($"Close: {record.Close}");
        Console.WriteLine($"Adjusted Close: {record.AdjustedClose}");
        Console.WriteLine($"Volume: {record.Volume}");
        Console.WriteLine($"Split Coefficient: {record.SplitCoefficient}");
        Console.WriteLine();
    }
}
```

</details>
</td>
    </tr>
        <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetForexRecordsAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves historical daily forex (foreign exchange) records for a given currency pair within a specified date range.

#### Parameters

* `string currency1`: The source currency (e.g., `"USD"`).
* `string currency2`: The target currency (e.g., `"EUR"`).
* `DateTime startDate`: The start date for the records.
* `DateTime? endDate`: (Optional) The end date for the records. Defaults to the current date.
* `CancellationToken token`: (Optional) A token to cancel the operation.

#### Returns

A task that resolves to an `IEnumerable<ForexRecord>`, with the following properties:

| Property            | Type       | Description                                                                              | Example     |
|---------------------|------------|------------------------------------------------------------------------------------------|-------------|
| `Date`              | `DateTime?` | The date of the forex record.                                                            | "2024-12-15"|
| `Open`              | `double?`  | The opening price of the currency pair for that date.                                     | 1.1215      |
| `High`              | `double?`  | The highest price of the currency pair for that date.                                    | 1.1250      |
| `Low`               | `double?`  | The lowest price of the currency pair for that date.                                     | 1.1180      |
| `Close`             | `double?`  | The closing price of the currency pair for that date.                                    | 1.1220      |

#### Example

```csharp
public async Task Run(IAlphaVantageService alphaVantageService)
{
    // Retrieve historical forex records for USD to EUR
    var forexRecords = await alphaVantageService.GetForexRecordsAsync("USD", "EUR", DateTime.Now.AddDays(-7));

    foreach (var record in forexRecords)
    {
        Console.WriteLine($"Date: {record.Date}");
        Console.WriteLine($"Open: {record.Open}");
        Console.WriteLine($"Close: {record.Close}");
    }
}
```

</details>
</td>
    </tr>
        <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetIntradayRecordsAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves intraday stock records for a given symbol within a specified date range and time interval.

#### Parameters

* `string symbol`: The stock symbol (e.g., `"AAPL"` for Apple).
* `DateTime startDate`: The start date for the records.
* `DateTime? endDate`: (Optional) The end date for the records. Defaults to the current date.
* `EInterval interval`: The time interval between data points. Default is 15 minutes. Possible values:
  - `Interval_1Min`
  - `Interval_5Min`
  - `Interval_15Min`
  - `Interval_30Min`
  - `Interval_60Min`
* `CancellationToken token`: (Optional) A token to cancel the operation.

#### Returns

A task that resolves to an `IEnumerable<IntradayRecord>`, with the following properties:

| Property         | Type       | Description                                                             | Example     |
|------------------|------------|-------------------------------------------------------------------------|-------------|
| `DateTime`       | `DateTime` | The date and time of the record.                                         | "2024-12-15 09:30" |
| `Open`           | `double`   | The opening price of the stock for that interval.                        | 145.32      |
| `High`           | `double`   | The highest price of the stock for that interval.                        | 147.10      |
| `Low`            | `double`   | The lowest price of the stock for that interval.                         | 144.98      |
| `Close`          | `double`   | The closing price of the stock for that interval.                        | 146.30      |
| `Volume`         | `long`     | The trading volume during that interval.                                 | 1234567    |


#### Example

```csharp
public async Task Run(IAlphaVantageService alphaVantageService)
{
    // Retrieve intraday stock records for AAPL with a 15-minute interval
    var intradayRecords = await alphaVantageService.GetIntradayRecordsAsync("AAPL", DateTime.Now.AddDays(-1), DateTime.Now, EInterval.Interval_15Min);

    foreach (var record in intradayRecords)
    {
        Console.WriteLine($"DateTime: {record.DateTime}");
        Console.WriteLine($"Open: {record.Open}");
        Console.WriteLine($"Close: {record.Close}");
    }
}
```

</details>
</td>
    </tr>
      </tbody>
</table>

## DataHub

[DataHub](https://www.datahub.io) is a popular source for accessing comprehensive data, including Nasdaq and S&P 500 companies.

<table>
  <thead>
    <tr>
      <th style="font-size: 16px;width: 1000px;">Methods</th>
    </tr>
  </thead>
  <tbody>
    <tr  style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetNasdaqInstrumentsAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves a collection of more than 4,000 Nasdaq instruments.

#### Parameters

* `CancellationToken token`: (Optional) Cancellation token.

#### Returns

A task that resolves to an `IEnumerable<NasdaqInstrument>` containing the following properties for each item:

| Property | Type       | Description                                       | Example       |
|----------|------------|---------------------------------------------------|---------------|
| `Symbol` | `string?`  | The ticker symbol of the instrument.              | TSLA          |
| `Name`   | `string?`  | The company name associated with the instrument. | Tesla, Inc.   |

#### Example

```csharp
public async Task Run(IDataHubService datahubService)
{
    var instruments = await datahubService.GetNasdaqInstrumentsAsync();
    foreach (var item in instruments)
    {
        Console.WriteLine($"Symbol: {item.Symbol}, Name: {item.Name}");
    }
}
```

</details>
</td>
    </tr>
        <tr style="background-color: transparent;">
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetSp500InstrumentsAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves a collection of S&P 500 instruments.

#### Parameters

* `CancellationToken token`: (Optional) Cancellation token.

#### Returns

A task that resolves to an `IEnumerable<Sp500Instrument>` containing the following properties for each item:

| Property             | Type         | Description                         | Example            |
|----------------------|--------------|-------------------------------------|--------------------|
| `Symbol`             | `string?`    | Ticker symbol of the instrument.    | TSLA               |
| `Name`               | `string?`    | Name of the instrument/company.     | Tesla, Inc.        |
| `Sector`             | `string?`    | Sector of the instrument.           | Automobile Manufacturers |
| `Price`              | `double?`    | Current price of the instrument.    | 345.16             |
| `PriceEarnings`      | `double?`    | Price-to-earnings ratio.            | 94.31              |
| `DividendYield`      | `double?`    | Dividend yield.                     | 0.89               |
| `EarningsShare`      | `double?`    | Earnings per share.                 | 3.66               |
| `FiftyTwoWeekLow`    | `double?`    | 52-week low price.                  | 338.8              |
| `FiftyTwoWeekHigh`   | `double?`    | 52-week high price.                 | 361.93             |
| `MarketCap`          | `long?`      | Market capitalization.              | 1107284384000      |
| `EBITDA`             | `long?`      | EBITDA value.                       | 13244000256        |
| `PriceSales`         | `double?`    | Price-to-sales ratio.               | 11.41              |
| `PriceBook`          | `double?`    | Price-to-book ratio.                | 15.82              |

#### Example

```csharp
public async Task Run(IDataHubService datahubService)
{
    var instruments = await datahubService.GetSp500InstrumentsAsync();
    foreach (var item in instruments)
    {
        Console.WriteLine($"Symbol: {item.Symbol}, Name: {item.Name}, Sector: {item.Sector}");
    }
}
```

</details>
</td>
    </tr>
      </tbody>
</table>

## Xetra

[Xetra](https://www.xetra.com/) is a leading European electronic trading platform, widely known for providing access to financial instruments listed on the Xetra market.

<table>
  <thead>
    <tr>
      <th style="font-size: 16px;width: 1000px;">Methods</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>
      <details><summary style="margin: 1px"><code style="font-size: 14px">GetInstrumentsAsync</code></summary>
      <div style="height: 1px; background-color: #ddd; width: 100%; margin-top: 10px;"></div>

#### Description

Retrieves a collection of more than 3,000 Xetra instruments.

#### Parameters

* `CancellationToken token`: (Optional) Cancellation token.

#### Returns

A task that resolves to an `IEnumerable<Instrument>` containing the following properties for each item:

| Property          | Type       | Description                                         | Example             |
|-------------------|------------|-----------------------------------------------------|---------------------|
| `Symbol`          | `string?`  | Ticker symbol of the financial instrument.          | TL0.DE              |
| `InstrumentStatus`| `string?`  | Current status of the instrument.                   | Active              |
| `InstrumentName`  | `string?`  | Full name of the financial instrument.              | TESLA INC. DL -,001 |
| `ISIN`            | `string?`  | International Securities Identification Number.      | US88160R1014        |
| `WKN`             | `string?`  | German securities identification number.            | 000A1CX3T           |
| `Mnemonic`        | `string?`  | Shorthand or mnemonic code for the instrument.      | TL0                 |
| `InstrumentType`  | `string?`  | Type of financial instrument (e.g., CS, ETF, ETN).  | CS                  |
| `Currency`        | `string?`  | Currency in which the instrument is traded.         | EUR                 |

#### Example

```csharp
public async Task Run(IXetraService xetraService)
{
    var instruments = await xetraService.GetInstrumentsAsync();
    foreach (var item in instruments)
    {
        Console.WriteLine($"Symbol: {item.Symbol}, Name: {item.InstrumentName}");
    }
}
```

</details>
</td>
    </tr>
  </tbody>
</table>

<div style="height: 1px;"></div>

## Disclaimer

Finance .NET is an open-source tool that uses Yahoo's and other publicly available APIs, and is intended for research and educational purposes.

#### When using the following services, you should refer to their respective terms of use

* Alpha Vantage: [Terms of use](https://www.alphavantage.co/)
* DataHub: [Terms of use (1)](https://github.com/datasets/s-and-p-500-companies), [Terms of use (2)](https://github.com/datasets/nasdaq-listings)
* Yahoo! Finance: [Terms of use (API)](https://policies.yahoo.com/us/en/yahoo/terms/product-atos/apiforydn/index.htm), [Terms of use](https://legal.yahoo.com/us/en/yahoo/terms/otos/index.html), [General Terms](https://policies.yahoo.com/us/en/yahoo/terms/index.htm)
* Xetra: [Terms of use](https://www.xetra.com/xetra-de/instrumente/alle-handelbaren-instrumente)

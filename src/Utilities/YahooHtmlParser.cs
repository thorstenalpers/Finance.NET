using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.XPath;
using Finance.Net.Enums;
using Finance.Net.Exceptions;
using Finance.Net.Models.Yahoo;
using Microsoft.Extensions.Logging;

namespace Finance.Net.Utilities;

/// <inheritdoc />
internal static class YahooHtmlParser
{
    public static Profile ParseProfile(IHtmlDocument document)
    {
        var descriptionElement = document.Body.SelectSingleNode("//section[header/h3[contains(text(), 'Description') or contains(text(), 'Summary')]]/p\n");
        var cntEmployeesElement = document.Body.SelectSingleNode("//dt[contains(text(), 'Employees')]/following-sibling::dd");
        var industryElement = document.Body.SelectSingleNode("//dt[contains(text(), 'Industry')]/following-sibling::a");
        var sectorElement = document.Body.SelectSingleNode("//dt[contains(text(), 'Sector')]/following-sibling::dd/a");
        var phoneElement = document.Body.SelectSingleNode("//a[@aria-label='phone number']");
        var websiteElement = document.Body.SelectSingleNode("//a[@aria-label='website link']");
        var addressElements = document.Body.SelectNodes("//div[contains(@class, 'address')]/div");
        var addressNameElement = document.Body.SelectSingleNode("//div[contains(@class, 'address')]/../../..//h3");

        var description = descriptionElement?.TextContent?.Trim();
        var cntEmployees = cntEmployeesElement?.TextContent?.Replace(",", "").Replace("-", "")?.Trim();
        var industry = industryElement?.TextContent?.Trim();
        var sector = sectorElement?.TextContent?.Trim();
        var phone = phoneElement?.TextContent;
        var website = websiteElement?.TextContent?.Trim();
        var addressLocation = addressElements.IsNullOrEmpty() ? null : string.Join("\n", addressElements.Select(div => div.TextContent.Trim()));
        var addressName = addressNameElement?.TextContent?.Trim();
        var address = string.IsNullOrEmpty(addressName) ? addressLocation : addressName + "\n" + addressLocation;
        var cntEmployeesNumber = Helper.ParseLong(cntEmployees);

        var result = new Profile
        {
            Description = description,
            CntEmployees = cntEmployeesNumber,
            Industry = industry,
            Sector = sector,
            Adress = address,
            Phone = phone,
            Website = website
        };

        var isNullObj = Helper.AreAllPropertiesNull(result);
        return isNullObj ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
    }

    public static List<Record> ParseHistoryRecords<T>(IHtmlDocument document, ILogger<T> logger)
    {
        var records = new List<Record>();
        var expectedHeaderSet = new HashSet<string>(["Date", "Open", "High", "Low", "Close", "Adj Close", "Volume"]);
        var headerMap = new Dictionary<string, int>();

        var table = document.QuerySelector("table.table") ?? throw new FinanceNetException("table is null");
        var headers = table.QuerySelectorAll("thead th")
              .Select(th =>
              {
                  th.QuerySelectorAll("span").ToList().ForEach(span => span.Remove());
                  return th.TextContent.Trim();
              })
              .ToList();
        for (var i = 0; i < headers.Count; i++)
        {
            headerMap[headers[i]] = i;
        }
        if (!expectedHeaderSet.IsSubsetOf(headerMap.Keys))
        {
            throw new FinanceNetException("Headers are missing");
        }

        foreach (var row in table.QuerySelectorAll("tbody tr"))
        {
            var cells = row.QuerySelectorAll("td").Select(td => td.TextContent.Trim()).ToArray();

            if (cells.Length == 7)
            {
                var dateString = cells[headerMap["Date"]];
                var openString = cells[headerMap["Open"]];
                var highString = cells[headerMap["High"]];
                var lowString = cells[headerMap["Low"]];
                var closeString = cells[headerMap["Close"]];
                var adjCloseString = cells[headerMap["Adj Close"]];
                var volumeString = cells[headerMap["Volume"]];

                var date = Helper.ParseDate(dateString);
                var open = Helper.ParseDecimal(openString);
                var high = Helper.ParseDecimal(highString);
                var low = Helper.ParseDecimal(lowString);
                var close = Helper.ParseDecimal(closeString);
                var adjClose = Helper.ParseDecimal(adjCloseString);
                var volume = Helper.ParseLong(volumeString);

                if (date == null)
                {
                    logger.LogWarning("invalid date {DateString}", dateString);
                    continue;
                }

                records.Add(new Record
                {
                    Date = date.Value,
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    AdjustedClose = adjClose,
                    Volume = volume
                });
            }
            else
            {
                logger.LogInformation("No records in row {Row}", row.TextContent);    // e.g. date + dividend (over all columns)
            }
        }
        return records.Count == 0 ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : records;
    }

    public static Dictionary<string, FinancialReport> ParseFinancialReports<T>(IHtmlDocument document, ILogger<T> logger)
    {
        var result = new Dictionary<string, FinancialReport>();
        var headers = document
            .Body.SelectNodes("//div[contains(@class, 'tableHeader')]//div[contains(@class, 'column')]")
            .Select(header => header.TextContent.Trim())
            .Where(e => e != "Breakdown")
            .ToList();
        if (headers.Count == 0)
        {
            throw new FinanceNetException("no content");
        }

        headers.RemoveAll(e => e.Contains(" - "));// remove commercial column
        foreach (var header in headers)
        {
            result.Add(header, new FinancialReport());
        }

        var rows = document
            .Body.SelectNodes("//div[contains(@class, 'tableBody')]//div[contains(@class, 'row ')]")
            .ToList();

        foreach (var row in rows)
        {
            var columns = row.ChildNodes.QuerySelectorAll("div.column").Select(e => e.TextContent.Trim()).ToList();

            if (columns.Count != headers.Count + 1)
            {
                throw new FinanceNetException($"Unknown table format of html");
            }

            var rowTitle = columns[0];
            var values = columns.Skip(1).Select(Helper.ParseDecimal).ToList();

            var propertyName = rowTitle.Replace(" ", "").Replace("&", "And");
            var propertyInfo = typeof(FinancialReport).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo != null)
            {
                for (var i = 0; i < headers.Count; i++)
                {
                    var header = headers[i];
                    var value = values[i];
                    var report = result[header];
                    propertyInfo.SetValue(report, value);
                }
            }
            else
            {
                logger.LogWarning("Unknown row property {RowTitle}.", rowTitle);
            }
        }
        return result.IsNullOrEmpty() ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : result;
    }

    public static Summary ParseSummary<T>(IHtmlDocument document, ILogger<T> logger)
    {
        var nameElement = document.Body.SelectSingleNode("//section/h1");
        var name = Helper.RemoveSymbolHeader(nameElement?.TextContent?.Trim());

        var askElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Ask')]]/span[2]");
        var askStr = askElement?.TextContent?.Trim();
        askStr = askStr == null ? null : Regex.Replace(askStr, @"\s*x\s*[0-9 -]+", "", RegexOptions.None, TimeSpan.FromSeconds(30))?.Trim();  // remove "x 100" of e.g. "415.81 x 100"
        var ask = Helper.ParseDecimal(askStr);

        var avgVolumeElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Avg. Volume')]]/span[2]");
        var avgVolume = Helper.ParseDecimal(avgVolumeElement?.TextContent?.Trim());

        var beta_5Y_MonthlyElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Beta (5Y Monthly)')]]/span[2]");
        var beta_5Y_Monthly = Helper.ParseDecimal(beta_5Y_MonthlyElement?.TextContent?.Trim());

        var bidElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Bid')]]/span[2]");
        var bidStr = bidElement?.TextContent?.Trim();
        bidStr = bidStr == null ? null : Regex.Replace(bidStr, @"\s*x\s*[0-9 -]+", "", RegexOptions.None, TimeSpan.FromSeconds(30))?.Trim();  // remove "x 100" of e.g. "415.81 x 100"
        var bid = Helper.ParseDecimal(bidStr);

        var daysRangeElement = document.Body.SelectSingleNode("//li[span[contains(text(), 's Range')]]/span[2]");
        var daysRange = daysRangeElement?.TextContent?.Trim()?.Split(" - ");
        var daysRange_Min = daysRange?.Length == 2 ? Helper.ParseDecimal(daysRange.FirstOrDefault()) : null;
        var daysRange_Max = daysRange?.Length == 2 ? Helper.ParseDecimal(daysRange.LastOrDefault()) : null;

        var earningsDateElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Earnings Date')]]/span[2]");
        var earningsDateStr = earningsDateElement?.TextContent?.Trim()?.Split(" - ");
        var earningsDate = earningsDateStr == null || earningsDateStr.Length == 0 ? null : Helper.ParseDate(earningsDateStr.FirstOrDefault());

        var ePS_TTMElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'EPS (TTM)')]]/span[2]");
        var ePS_TTM = Helper.ParseDecimal(ePS_TTMElement?.TextContent?.Trim());

        var ex_DividendDateElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Ex-Dividend Date')]]/span[2]");
        var ex_DividendDate = Helper.ParseDate(ex_DividendDateElement?.TextContent?.Replace("-", "")?.Trim());

        var forward_DividendAndYieldElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Forward Dividend & Yield')]]/span[2]");
        var dividentAndYield = forward_DividendAndYieldElement?.TextContent?.Trim().Split(" ")?.Select(e => e.Replace("(", "").Replace(")", "").Replace("%", ""));
        var forward_Dividend = dividentAndYield?.Count() == 2 ? Helper.ParseDecimal(dividentAndYield.FirstOrDefault()) : null;
        var forward_Yield = dividentAndYield?.Count() == 2 ? Helper.ParseDecimal(dividentAndYield.LastOrDefault()) : null;

        var marketCap_IntradayElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Market Cap (intraday)')]]/span[2]");
        var marketCap_Intraday = Helper.ParseDecimal(marketCap_IntradayElement?.TextContent?.Trim());

        var marketTimeNoticeElement = document.Body.SelectSingleNode("//div[@slot='marketTimeNotice']");
        var marketTimeNotice = marketTimeNoticeElement?.TextContent?.Trim();

        var oneYearTargetEstElement = document.Body.SelectSingleNode("//li[span[contains(text(), '1y Target Est')]]/span[2]");
        var oneYearTargetEst = Helper.ParseDecimal(oneYearTargetEstElement?.TextContent?.Trim());

        var openElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Open')]]/span[2]");
        var open = Helper.ParseDecimal(openElement?.TextContent?.Trim());

        var pE_Ratio_TTMElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'PE Ratio (TTM)')]]/span[2]");
        var pE_Ratio_TTM = Helper.ParseDecimal(pE_Ratio_TTMElement?.TextContent?.Trim());

        var previousCloseElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Previous Close')]]/span[2]");
        var previousClose = Helper.ParseDecimal(previousCloseElement?.TextContent?.Trim());

        var volumeElement = document.Body.SelectSingleNode("//li[span[contains(text(), 'Volume')]]/span[2]");
        var volume = Helper.ParseDecimal(volumeElement?.TextContent?.Trim());

        var weekRange52Element = document.Body.SelectSingleNode("//li[span[contains(text(), '52 Week Range')]]/span[2]");
        var weekRange52 = weekRange52Element?.TextContent?.Trim()?.Split(" - ");
        var weekRange52_Min = weekRange52?.Length == 2 ? Helper.ParseDecimal(weekRange52.FirstOrDefault()) : null;
        var weekRange52_Max = weekRange52?.Length == 2 ? Helper.ParseDecimal(weekRange52.LastOrDefault()) : null;

        if (previousClose == null)
        {
            logger.LogWarning("invalid date {PreviousClose}", previousClose);
        }
        var summary = new Summary
        {
            Name = name,
            Ask = ask,
            AvgVolume = avgVolume,
            Beta_5Y_Monthly = beta_5Y_Monthly,
            Bid = bid,
            DaysRange_Max = daysRange_Max,
            DaysRange_Min = daysRange_Min,
            EarningsDate = earningsDate,
            EPS_TTM = ePS_TTM,
            Ex_DividendDate = ex_DividendDate,
            Forward_Dividend = forward_Dividend,
            Forward_Yield = forward_Yield,
            MarketCap_Intraday = marketCap_Intraday,
            MarketTimeNotice = marketTimeNotice,
            OneYearTargetEst = oneYearTargetEst,
            Open = open,
            PE_Ratio_TTM = pE_Ratio_TTM,
            PreviousClose = previousClose,
            Volume = volume,
            WeekRange52_Max = weekRange52_Max,
            WeekRange52_Min = weekRange52_Min
        };
        var isNullObj = Helper.AreAllPropertiesNull(summary);
        return isNullObj ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : summary;
    }

    public static List<Instrument> ParseSymbols<T>(IHtmlDocument document, EAssetType type, ILogger<T> logger)
    {
        var instruments = new List<Instrument>();
        var expectedHeaderSet = new HashSet<string>(["Symbol"]);
        var headerMap = new Dictionary<string, int>();

        var table = document.QuerySelector("table.markets-table") ?? throw new FinanceNetException("No table found");
        var headers = table.QuerySelectorAll("thead th")
              .Select(th =>
              {
                  th.QuerySelectorAll("span").ToList().ForEach(span => span.Remove());
                  return th.TextContent.Trim();
              })
              .ToList();
        for (var i = 0; i < headers.Count; i++)
        {
            headerMap[headers[i]] = i;
        }
        if (!expectedHeaderSet.IsSubsetOf(headerMap.Keys))
        {
            throw new FinanceNetException("Headers are missing");
        }

        foreach (var row in table.QuerySelectorAll("tbody tr"))
        {
            var cells = row.QuerySelectorAll("td").Select(td => td.TextContent.Trim()).ToArray();

            if (cells.Length >= 1)
            {
                var symbol = cells[headerMap["Symbol"]];
                if (symbol == null)
                {
                    logger.LogWarning("invalid symbol {Symbol}", symbol);
                    continue;
                }

                instruments.Add(new Instrument
                {
                    Symbol = symbol,
                    InstrumentType = type
                });
            }
            else
            {
                logger.LogWarning("Invalid row {Row}", row.TextContent);
            }
        }

        return instruments.Count == 0 ? throw new FinanceNetException(Constants.ValidationMsgAllFieldsEmpty) : instruments;
    }
}
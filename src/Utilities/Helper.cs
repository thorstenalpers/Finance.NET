using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Finance.Net.Enums;
using Microsoft.Extensions.Logging;

namespace Finance.Net.Utilities;

internal static class Helper
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
    {
        return list?.Any() != true;
    }

    public static long? ToUnixTime(DateTime? dateTime)
    {
        return dateTime == null ? null : (long)(dateTime.Value - DateTime.UnixEpoch).TotalSeconds;
    }

    public static DateTime? UnixToDateTime(long? unixTimeSeconds)
    {
        return unixTimeSeconds == null ? null : DateTime.UnixEpoch.AddSeconds(unixTimeSeconds.Value).ToUniversalTime();
    }
    public static DateTime? UnixMillisecsToDate(long? unixTimeMilliseconds)
    {
        return unixTimeMilliseconds == null ? null : DateTime.UnixEpoch.AddMilliseconds(unixTimeMilliseconds.Value).ToUniversalTime();
    }

    public static long? ParseLong(string? numberString)
    {
        var cleanedNumber = numberString?.Replace(",", "");
        if (string.IsNullOrWhiteSpace(cleanedNumber) || cleanedNumber.All(e => e == '-'))
        {
            return null;
        }
        if (long.TryParse(cleanedNumber, out var result))
        {
            return result;
        }
        return (long)ParseWithMultiplier(cleanedNumber);
    }

    public static decimal? ParseDecimal(string? numberString)
    {
        var cleanedNumber = numberString?.Replace(",", "");
        if (string.IsNullOrWhiteSpace(cleanedNumber) || cleanedNumber.All(e => e == '-'))
        {
            return null;
        }
        if (decimal.TryParse(cleanedNumber, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return ParseWithMultiplier(cleanedNumber);
    }

    private static decimal ParseWithMultiplier(string? cleanedNumber)
    {
        // otherwise has numer format such as 100.00Mio?
        var match = new Regex("([0-9.,-]+)([A-Za-z]+)", RegexOptions.None, TimeSpan.FromSeconds(30)).Match(cleanedNumber);
        if (match.Groups.Count <= 2)
        {
            throw new FormatException($"Unknown format of {cleanedNumber}");
        }
        if (!decimal.TryParse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            throw new FormatException($"Unknown format of {cleanedNumber}");
        }
        var mulStr = match.Groups[2].Value;
        return mulStr switch
        {
            "Trl." or "Trl" or "T" => result * 1_000_000_000_000_000,
            "Bio." or "Bio" or "B" => result * 1_000_000_000_000,
            "Mrd." or "Mrd" => result * 1_000_000_000,
            "Mio." or "Mio" or "M" => result * 1_000_000,
            "k" or "K" => result * 1_000,
            _ => throw new FormatException($"Unknown multiplikator={mulStr} of {cleanedNumber}")
        };
    }

    public static DateTime? ParseDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        // List of possible formats
        var formats = new[]
        {
          "MMM d, yyyy",    // "Nov 1, 2024"
					"MMM dd, yyyy",   // "Nov 01, 2024"
					"MMMM d, yyyy",  // "November 1, 2024"
					"MMMM dd, yyyy",  // "November 01, 2024"
					"yyyy-MM-d",     // "2024-11-1"
					"yyyy-MM-dd",     // "2024-11-01"
					"yyyy/MM/d",     // "2024/11/1"
					"yyyy/MM/dd",     // "2024/11/01"
				};

        // Try parsing the date using each format
        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }
        throw new FormatException($"Invalid date format {dateString}");
    }

    public static string GetDescription(this EInterval value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
        {
            return "";
        }

        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute.Description;
    }
    public static string CreateRandomUserAgent()
    {
        return CreateRandomUserAgent(RandomNumberGenerator.GetInt32);
    }

    public static string CreateRandomUserAgent(Func<int, int> random)
    {
        List<string> firefoxVersions = [ "133.0",
                        "132.0", "132.0.1", "132.0.2",
                        "131.0", "131.0.1", "131.0.2",
                        "130.0", "130.0.1",
                        "129.0", "129.0.1", "129.0.2"];

        List<string> operatingSystems = [
          "Windows NT 10.0; Win64; x64",
          "Linux x86_64",
          "X11; Ubuntu; Linux x86_64"];

        var allAgents = new List<string>();
        foreach (var firefoxVersion in firefoxVersions)
        {
            foreach (var operatingSystem in operatingSystems)
            {
                var userAgent = $"Mozilla/5.0 ({operatingSystem}; rv:{firefoxVersion}) Gecko/20100101 Firefox/{firefoxVersion}";
                allAgents.Add(userAgent);
            }
        }
        var index = random(allAgents.Count);
        return allAgents[index];
    }

    public static bool AreAllPropertiesNull<T>(T obj)
    {
        if (obj is null)
        {
            return true;
        }
        var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        return properties.All(e => e.GetValue(obj) == null);
    }

    public static string? Minify(this string strXmlContent)
    {
        return strXmlContent == null ? null : Regex.Replace(strXmlContent, @"\s+", " ", RegexOptions.None, TimeSpan.FromSeconds(30));
    }

    public static async Task<IHtmlDocument> FetchHtmlDocumentAsync<T>(HttpClient httpClient, ILogger<T> logger, string url, CancellationToken token)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var htmlContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        logger?.LogDebug("htmlContent={HtmlContent}", htmlContent.Minify());
        return new AngleSharp.Html.Parser.HtmlParser().ParseDocument(htmlContent);
    }

    public static async Task<string> FetchJsonDocumentAsync<T>(HttpClient httpClient, ILogger<T> logger, string url, CancellationToken token)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        logger?.LogDebug("jsonContent={JsonContent}", jsonContent.Minify());
        return jsonContent;
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NetFinance.Application.Utilities;

internal static class Helper
{
	public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
	{
		return list?.Any() != true;
	}
	public static long ToUnixTimestamp(DateTime dateTime)
	{
		var unixEpoch = new DateTime(1970, 1, 1);
		return (long)(dateTime - unixEpoch).TotalSeconds;
	}

	public static long? ParseLong(string numberString)
	{
		string cleanedNumber = numberString.Replace(",", "");
		if (string.IsNullOrWhiteSpace(cleanedNumber) || cleanedNumber == "-") return null;

		if (long.TryParse(cleanedNumber, out long result))
		{
			return result;
		}
		throw new FormatException($"Invalid long format {numberString}");
	}

	public static decimal? ParseDecimal(string numberString)
	{
		string cleanedNumber = numberString.Replace(",", "");
		if (string.IsNullOrWhiteSpace(cleanedNumber) || cleanedNumber == "-") return null;
		if (decimal.TryParse(cleanedNumber, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
		{
			return result;
		}
		throw new FormatException($"Invalid decimal format {numberString}");
	}

	public static DateTime ParseDate(string dateString)
	{
		// List of possible formats
		var formats = new[]
		{
			"MMM d, yyyy",    // "Nov 2, 2024"
			"MMM dd, yyyy",   // "Nov 29, 2024"
            "MMMM dd, yyyy",  // "November 29, 2024"
            "MM/dd/yyyy",     // "11/29/2024"
            "yyyy-MM-dd",     // "2024-11-29"
            "yyyy/MM/dd",     // "2024/11/29"
            "dd/MM/yyyy",     // "29/11/2024"
        };

		// Try parsing the date using each format
		foreach (var format in formats)
		{
			if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
			{
				return result;
			}
		}
		throw new FormatException($"Invalid date format {dateString}");
	}

	public static string CreateRandomUserAgent()
	{
		var random = new Random();

		List<string> firefoxVersions = [ "133.0",
										 "132.0", "132.0.1", "132.0.2",
										 "131.0", "131.0.1", "131.0.2",
										 "130.0", "130.0.1",
										 "129.0", "129.0.1", "129.0.2",
										 "128.0", "128.0.2", "128.0.3","128.1.0", "128.2.0", "128.3.0", "128.3.1"];

		var operatingSystems = new List<string>
		{
			"Windows NT 10.0; Win64; x64",
			"Windows NT 6.1; WOW64",
			"Macintosh; Intel Mac OS X 10_15_7",
			"Macintosh; Intel Mac OS X 10_14_6",
			"Linux x86_64",
			"Linux i686"
		};

		var firefoxUserAgents = new List<string>();

		for (int i = 0; i < 1000; i++)
		{
			string firefoxVersion = firefoxVersions[random.Next(firefoxVersions.Count)];
			string operatingSystem = operatingSystems[random.Next(operatingSystems.Count)];
			string userAgent = $"Mozilla/5.0 ({operatingSystem}; rv:{firefoxVersion}) Gecko/20100101 Firefox/{firefoxVersion}";

			firefoxUserAgents.Add(userAgent);
		}
		var index = random.Next(firefoxUserAgents.Count);
		return firefoxUserAgents[index];
	}
}
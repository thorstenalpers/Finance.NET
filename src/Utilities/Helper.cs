﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Finance.Net.Models.AlphaVantage;

namespace Finance.Net.Utilities;

public static class Helper
{
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
		{
				return list?.Any() != true;
		}

		public static long? ToUnixTime(DateTime? dateTime)
		{
				if (dateTime == null)
				{
						return null;
				}

				var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				return (long)(dateTime.Value - unixEpoch).TotalSeconds;
		}

		public static DateTime? UnixToDateTime(long? unixTimeSeconds)
		{
				if (unixTimeSeconds == null)
				{
						return null;
				}

				var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				return epoch.AddSeconds(unixTimeSeconds.Value).ToUniversalTime();
		}
		public static DateTime? UnixMillisecsToDate(long? unixTimeMilliseconds)
		{
				if (unixTimeMilliseconds == null) return null;
				var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				return epoch.AddMilliseconds(unixTimeMilliseconds.Value).ToUniversalTime();
		}

		public static long? ParseLong(string? numberString)
		{
				var cleanedNumber = numberString?.Replace(",", "");
				return string.IsNullOrWhiteSpace(cleanedNumber) || cleanedNumber.All(e => e == '-')
						? null
						: long.TryParse(cleanedNumber, out long result) ? result : (long)ParseWithMultiplier(cleanedNumber);
		}

		public static decimal? ParseDecimal(string? numberString)
		{
				var cleanedNumber = numberString?.Replace(",", "");
				return string.IsNullOrWhiteSpace(cleanedNumber) || cleanedNumber.All(e => e == '-')
						? null
						: decimal.TryParse(cleanedNumber, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result)
						? result
						: ParseWithMultiplier(cleanedNumber);
		}

		private static decimal ParseWithMultiplier(string? cleanedNumber)
		{
				// otherwise has numer format such as 100.00Mio?
				var match = new Regex("([0-9.,-]+)([A-Za-z]+)").Match(cleanedNumber);
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
						"Trl." or "Trl" or "T" => result * 1000000000000000,
						"Bio." or "Bio" or "B" => result * 1000000000000,
						"Mrd." or "Mrd" => result * 1000000000,
						"Mio." or "Mio" or "M" => result * 1000000,
						"k" or "K" => result * 1000,
						_ => throw new FormatException($"Unknown multiplikator={mulStr} of {cleanedNumber}")
				};
		}

		public static DateTime? ParseDate(string? dateString)
		{
				if (string.IsNullOrWhiteSpace(dateString)) return null;

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
						if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
						{
								return result;
						}
				}
				throw new FormatException($"Invalid date format {dateString}");
		}

		public static string GetDescription(this EInterval value)
		{
				var field = value.GetType().GetField(value.ToString());
				var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
				return attribute.Description;
		}
		public static string CreateRandomUserAgent()
		{
				return CreateRandomUserAgent(new Random());
		}

		public static string CreateRandomUserAgent(Random random)
		{
				random ??= new Random();
				List<string> firefoxVersions = [ "133.0",
																				 "132.0", "132.0.1", "132.0.2",
																				 "131.0", "131.0.1", "131.0.2",
																				 "130.0", "130.0.1",
																				 "129.0", "129.0.1", "129.0.2"];

				List<string> operatingSystems = [
					 "Windows NT 10.0; Win64; x64",
						"Linux x86_64",
						"X11; Ubuntu; Linux x86_64"
					];
				var allAgents = new List<string>();
				foreach (var firefoxVersion in firefoxVersions)
				{
						foreach (var operatingSystem in operatingSystems)
						{
								string userAgent = $"Mozilla/5.0 ({operatingSystem}; rv:{firefoxVersion}) Gecko/20100101 Firefox/{firefoxVersion}";
								allAgents.Add(userAgent);
						}
				}
				var index = random.Next(allAgents.Count);
				return allAgents[index];
		}

		public static bool AreAllFieldsNull<T>(T obj)
		{
				if (obj == null)
				{
						return true;
				}
				var fields = obj.GetType().GetFields(
						System.Reflection.BindingFlags.Instance |
						System.Reflection.BindingFlags.Public |
						System.Reflection.BindingFlags.NonPublic).ToList();
				return fields.All(field => field.GetValue(obj) == null);
		}

		public static string Minify(this string strXmlContent)
		{
				return string.IsNullOrWhiteSpace(strXmlContent) ? strXmlContent : Regex.Replace(strXmlContent, @"\s+", " ");
		}
}
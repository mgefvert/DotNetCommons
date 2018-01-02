using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.Text
{
    public static class StringExtensions
    {
        public static bool Contains(this StringBuilder builder, char c)
        {
            for (var i = 0; i < builder.Length; i++)
                if (builder[i] == c)
                    return true;

            return false;
        }
        
        public static string Left(this string value, int count)
        {
            return string.IsNullOrEmpty(value)
              ? string.Empty
              : Mid(value, 0, count);
        }

        public static string LeftEllipsis(this string value, int count)
        {
            var result = Left(value, count);
            if (value != null && value.Length > count)
                result += "…";

            return result;
        }

        public static bool Like(this string value, string compare)
        {
            return string.Equals(value, compare, StringComparison.CurrentCultureIgnoreCase);
        }

        public static string Mid(this string value, int offset)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return offset < value.Length ? value.Substring(offset) : string.Empty;
        }

        public static string Mid(this string value, int offset, int count)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (offset < 0)
                offset = 0;

            var len = value.Length;
            if (offset > len)
                return string.Empty;

            var maxLeft = len - offset;
            if (count > maxLeft)
                count = maxLeft;

            return count > 0 ? value.Substring(offset, count) : string.Empty;
        }

        public static decimal ParseDecimal(this string value, CultureInfo culture, decimal defaultValue = 0)
        {
            return decimal.TryParse((value ?? "").Trim(), NumberStyles.Number, culture, out var result) 
                ? result 
                : defaultValue;
        }

        public static decimal ParseDecimal(this string value, decimal defaultValue = 0)
        {
            return ParseDecimal(value, CultureInfo.CurrentCulture, defaultValue);
        }

        public static decimal ParseDecimalInvariant(this string value, decimal defaultValue = 0)
        {
            return ParseDecimal(value, CultureInfo.InvariantCulture, defaultValue);
        }

        public static double ParseDouble(this string value, CultureInfo culture, double defaultValue = 0)
        {
            return double.TryParse((value ?? "").Trim(), NumberStyles.Number, culture, out var result)
                ? result 
                : defaultValue;
        }

        public static double ParseDouble(this string value, double defaultValue = 0)
        {
            return ParseDouble(value, CultureInfo.CurrentCulture, defaultValue);
        }

        public static double ParseDoubleInvariant(this string value, double defaultValue = 0)
        {
            return ParseDouble(value, CultureInfo.InvariantCulture, defaultValue);
        }

        public static int ParseInt(this string value, int defaultValue = 0)
        {
            return int.TryParse((value ?? "").Trim(), out var result) ? result : defaultValue;
        }

        public static long ParseLong(this string value, long defaultValue = 0)
        {
            return long.TryParse((value ?? "").Trim(), out var result) ? result : defaultValue;
        }

        public static string Right(this string value, int count)
        {
            return string.IsNullOrEmpty(value)
              ? string.Empty
              : Mid(value, value.Length - count, count);
        }

        public static IEnumerable<string> TrimAndFilter(this IEnumerable<string> strings)
        {
            return strings
                .Select(x => x?.Trim())
                .Where(x => !string.IsNullOrEmpty(x));
        }
    }
}

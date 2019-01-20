using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text
{
    public static class StringExtensions
    {
        /// <summary>
        /// Test whether a StringBuilder contains a particular character.
        /// </summary>
        /// <param name="builder">StringBuilder to query.</param>
        /// <param name="c">Character to search for.</param>
        /// <returns>true if the character is found, otherwise false.</returns>
        public static bool Contains(this StringBuilder builder, char c)
        {
            for (var i = 0; i < builder.Length; i++)
                if (builder[i] == c)
                    return true;

            return false;
        }

        public static string FirstLine(this string value)
        {
            var n = (value ?? "").IndexOfAny(new[] { '\r', '\n' });
            return n == -1 ? value : (value ?? "").Substring(0, n).Trim();
        }

        /// <summary>
        /// Take the left n characters from a string, possibly returning less than
        /// the full number of characters if there aren't enough in the string. If the
        /// source string is null, this function will simply return an empty string.
        /// </summary>
        /// <param name="value">String to operate on.</param>
        /// <param name="count">Number of characters to take. If this number is negative, it will
        ///     take all but the last n characters.</param>
        /// <returns>The n characters from the left of the source string.</returns>
        public static string Left(this string value, int count)
        {
            if (count < 0)
                count = (value?.Length ?? 0) + count;

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

        public static bool ParseBoolean(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (bool.TryParse(value, out var result))
                return result;

            return long.TryParse(value, out var longresult) && longresult != 0;
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

        /// <summary>
        /// Take the right n characters from a string, possibly returning less than
        /// the full number of characters if there aren't enough in the string. If the
        /// source string is null, this function will simply return an empty string.
        /// </summary>
        /// <param name="value">String to operate on.</param>
        /// <param name="count">Number of characters to take. If this number is negative, it will
        ///     take all but the first n characters.</param>
        /// <returns>The n characters from the right of the source string.</returns>
        public static string Right(this string value, int count)
        {
            if (count < 0)
                count = (value?.Length ?? 0) + count;

            return string.IsNullOrEmpty(value)
              ? string.Empty
              : Mid(value, value.Length - count, count);
        }

        public static string StartUppercase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length == 1
                ? value.ToUpper()
                : char.ToUpper(value[0]) + value.Substring(1, value.Length - 1);
        }

        public static IEnumerable<string> TrimAndFilter(this IEnumerable<string> strings)
        {
            return strings
                .Select(x => x?.Trim())
                .Where(x => !string.IsNullOrEmpty(x));
        }
    }
}

﻿using System;
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
        public static IEnumerable<string> BreakUp(this string value, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive.");

            for (var pos = 0; pos < value.Length; pos += length)
                yield return value.Mid(pos, length);
        }


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

        /// <summary>
        /// Get the first line from a text separated by CRLF.
        /// </summary>
        /// <param name="value">Line to search</param>
        /// <returns>The first line.</returns>
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

        /// <summary>
        /// Get count characters from the left, adding an ellipsis (...) symbol if there's
        /// more text beyond that.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string LeftEllipsis(this string value, int count)
        {
            var result = Left(value, count);
            if (value != null && value.Length > count)
                result += "…";

            return result;
        }

        /// <summary>
        /// Compare text according to the current culture, case insensitive.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public static bool Like(this string value, string compare)
        {
            return string.Equals(value, compare, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Mask the left portion of a string by replacing the characters with a mask character.
        /// </summary>
        /// <param name="value">String to work on.</param>
        /// <param name="length">Number of characters to replace.</param>
        /// <param name="mask">The character to mask with, e.g. '*'</param>
        /// <returns>A new, masked string</returns>
        public static string MaskLeft(this string value, int length, char mask)
        {
            return value.Left(-length) + new string(mask, length);
        }

        /// <summary>
        /// Mask the right portion of a string by replacing the characters with a mask character.
        /// </summary>
        /// <param name="value">String to work on.</param>
        /// <param name="length">Number of characters to replace.</param>
        /// <param name="mask">The character to mask with, e.g. '*'</param>
        /// <returns>A new, masked string</returns>
        public static string MaskRight(this string value, int length, char mask)
        {
            return value.Right(-length) + new string(mask, length);
        }

        /// <summary>
        /// Get a substring from the middle of the text to the end. If the offset is outside
        /// of the string, an empty string will be returned.
        /// </summary>
        /// <param name="value">String to work with</param>
        /// <param name="offset">Zero-based offset</param>
        public static string Mid(this string value, int offset)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return offset < value.Length ? value.Substring(offset) : string.Empty;
        }

        /// <summary>
        /// Get a substring from the middle of the text. If the offset or count is outside
        /// of the string, as many characters as was found will be returned.
        /// </summary>
        /// <param name="value">String to work with</param>
        /// <param name="offset">Zero-based offset</param>
        /// <param name="count">Number of characters to extract</param>
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

        /// <summary>
        /// Returns null if the string IsNullOrEmpty.
        /// </summary>
        /// <param name="value">String to test.</param>
        /// <returns>Null if the string is empty, otherwise the original string.</returns>
        public static string NullIfEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// Parse a string to a boolean. Handles empty strings (=false), numbers, or
        /// the common "true"/"false" case.
        /// </summary>
        public static bool ParseBoolean(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value), "Null or empty string can not recognized as a boolean.");

            if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("t", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("y", StringComparison.OrdinalIgnoreCase))
                return true;

            if (value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("f", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("n", StringComparison.OrdinalIgnoreCase))
                return false;

            if (bool.TryParse(value, out var result))
                return result;

            if (long.TryParse(value, out var longresult))
                return longresult != 0;

            throw new ArgumentOutOfRangeException(nameof(value), $"String '{value}' can not recognized as a boolean.");
        }

        /// <summary>
        /// Parse a decimal value according to NumberStyles.Number and a given culture.
        /// </summary>
        public static decimal ParseDecimal(this string value, CultureInfo culture, decimal defaultValue = 0)
        {
            return decimal.TryParse((value ?? "").Trim(), NumberStyles.Number, culture, out var result)
                ? result
                : defaultValue;
        }

        /// <summary>
        /// Parse a decimal value according to NumberStyles.Number and the current culture.
        /// </summary>
        public static decimal ParseDecimal(this string value, decimal defaultValue = 0)
        {
            return ParseDecimal(value, CultureInfo.CurrentCulture, defaultValue);
        }

        /// <summary>
        /// Parse a decimal value according to NumberStyles.Number and the invariant culture.
        /// </summary>
        public static decimal ParseDecimalInvariant(this string value, decimal defaultValue = 0)
        {
            return ParseDecimal(value, CultureInfo.InvariantCulture, defaultValue);
        }

        /// <summary>
        /// Parse a double value according to NumberStyles.Number and a given culture.
        /// </summary>
        public static double ParseDouble(this string value, CultureInfo culture, double defaultValue = 0)
        {
            return double.TryParse((value ?? "").Trim(), NumberStyles.Number, culture, out var result)
                ? result
                : defaultValue;
        }

        /// <summary>
        /// Parse a double value according to NumberStyles.Number and the current culture.
        /// </summary>
        public static double ParseDouble(this string value, double defaultValue = 0)
        {
            return ParseDouble(value, CultureInfo.CurrentCulture, defaultValue);
        }

        /// <summary>
        /// Parse a double value according to NumberStyles.Number and the invariant culture.
        /// </summary>
        public static double ParseDoubleInvariant(this string value, double defaultValue = 0)
        {
            return ParseDouble(value, CultureInfo.InvariantCulture, defaultValue);
        }

        /// <summary>
        /// Parse a integer value.
        /// </summary>
        public static int ParseInt(this string value, int defaultValue = 0)
        {
            return int.TryParse((value ?? "").Trim(), out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Parse a long value.
        /// </summary>
        public static long ParseLong(this string value, long defaultValue = 0)
        {
            return long.TryParse((value ?? "").Trim(), out var result) ? result : defaultValue;
        }

        public static string Repeat(this string value, int count)
        {
            switch (count)
            {
                case 0: return "";
                case 1: return value;
                default:
                    {
                        var sb = new StringBuilder(value.Length * count);
                        for (var i = 0; i < count; i++)
                            sb.Append(value);
                        return sb.ToString();
                    }
            }
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

        /// <summary>
        /// Return the string with the first letter in uppercase.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string StartUppercase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length == 1
                ? value.ToUpper()
                : char.ToUpper(value[0]) + value.Substring(1, value.Length - 1);
        }

        /// <summary>
        /// Combine Trim() and IsNullOrEmpty() on a sequence of strings.
        /// </summary>
        public static IEnumerable<string> TrimAndFilter(this IEnumerable<string> strings)
        {
            return strings
                .Select(x => x?.Trim())
                .Where(x => !string.IsNullOrEmpty(x));
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

[Flags]
public enum Spacing
{
    Trim = 1,
    ReplaceTabs = 2,
    ReplaceCrLfs = 4,
    ReplaceDoubleSpaces = 8,
    ReplaceAll = 15
}

public static partial class CommonStringExtensions
{
    /// <summary>
    /// Break up a string in chunks of a given length.
    /// </summary>
    public static IEnumerable<string> BreakUp(this string? value, int length)
    {
        if (string.IsNullOrEmpty(value))
            yield break;

        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive.");

        for (var pos = 0; pos < value.Length; pos += length)
            yield return value.Mid(pos, length);
    }

    /// <summary>
    /// Chomp off the first word from a string. Quoted substring are regarded as a single word.
    /// </summary>
    /// <param name="remaining">The new string lacking the first word</param>
    /// <param name="value">Input string</param>
    /// <param name="separator">Separator to use, default space</param>
    /// <param name="quote">Quote character to use, default "</param>
    /// <returns>Chopped off word or quoted phrase (without quotes)</returns>
    public static string? Chomp(this string? value, out string? remaining, char separator = ' ', char quote = '"')
    {
        remaining = "";
        value = value?.Trim(separator);
        if (string.IsNullOrEmpty(value))
            return null;

        var inQuote = false;
        string? result = null;
        for (var i = 0; i < value.Length; i++)
        {
            if (inQuote)
            {
                if (value[i] == quote)
                    inQuote = false;
                continue;
            }

            if (value[i] == quote)
            {
                inQuote = true;
                continue;
            }

            if (value[i] == separator)
            {
                remaining = value.Mid(i + 1).Trim(separator);
                result = value.Left(i);
                break;
            }
        }

        result ??= value;

        if (result.StartsWith(quote) && result.EndsWith(quote))
            result = result.Mid(1, result.Length - 2);

        return result;
    }

    /// <summary>
    /// Splits a string into multiple segments based on a specified separator and quote character, extracting each part sequentially.
    /// </summary>
    /// <param name="value">The input string to split. Can be null or empty.</param>
    /// <param name="separator">The character used as the delimiter between segments. Defaults to a space (' ').</param>
    /// <param name="quote">The character used to denote quoted sections, which may include the separator within them. Defaults to a double quote ('"').</param>
    /// <returns>An enumerable collection of strings containing the extracted segments. Empty if the input string is null or empty.</returns>
    public static IEnumerable<string> ChompAll(this string? value, char separator = ' ', char quote = '"')
    {
        while (!string.IsNullOrEmpty(value))
        {
            var word = value.Chomp(out value, separator, quote);
            if (word != null)
                yield return word;
        }
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
    /// Compare text according to the current culture, case insensitive.
    /// </summary>
    public static bool ContainsInsensitive(this string? value, string? compare)
    {
        return compare != null && (value?.Contains(compare, StringComparison.CurrentCultureIgnoreCase) ?? false);
    }

    /// <summary>
    /// Converts a string from LF line-feeds to CRLF. Is tolerant of the existing format as long as it's either
    /// LF or CRLF.
    /// </summary>
    public static string? ConvertToCrLf(this string? value)
    {
        return value?
            .Replace("\r", "")
            .Replace("\n", "\r\n");
    }

    /// <summary>
    /// Converts a string from CRLF line-feeds to LF. Is tolerant of the existing format as long as it's either
    /// LF or CRLF.
    /// </summary>
    public static string? ConvertToLf(this string? value)
    {
        return value?.Replace("\r\n", "\n");
    }

    /// <summary>
    /// Compare text according to the current culture, case insensitive.
    /// </summary>
    public static bool EqualsInsensitive(this string? value, string? compare)
    {
        return string.Equals(value, compare, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Get the first line from a text separated by CRLF.
    /// </summary>
    /// <param name="value">Line to search</param>
    /// <returns>The first line.</returns>
    public static string? FirstLine(this string? value)
    {
        var n = (value ?? "").IndexOfAny(['\r', '\n']);
        return n == -1 ? value : (value ?? "")[..n].Trim();
    }

    /// <summary>
    /// Determines whether the specified string is empty or null.
    /// </summary>
    /// <param name="value">The string to check for null or emptiness.</param>
    /// <returns>True if the string is null or empty; otherwise, false.</returns>
    public static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// Determines whether the specified string is set to a value different from null or empty.
    /// </summary>
    /// <param name="value">The string to evaluate.</param>
    /// <returns>Returns true if the string is not null or empty; otherwise, false.</returns>
    public static bool IsSet([NotNullWhen(true)] this string? value) => !string.IsNullOrEmpty(value);

    /// <summary>
    /// Take the left n characters from a string, possibly returning less than
    /// the full number of characters if there aren't enough in the string. If the
    /// source string is null, this function will simply return an empty string.
    /// </summary>
    /// <param name="value">String to operate on.</param>
    /// <param name="count">Number of characters to take. If this number is negative, it will
    ///     take all but the last n characters.</param>
    /// <returns>The n characters from the left of the source string.</returns>
    public static string Left(this string? value, int count)
    {
        if (count < 0)
            count = (value?.Length ?? 0) + count;

        return string.IsNullOrEmpty(value)
            ? string.Empty
            : Mid(value, 0, count);
    }

    /// <summary>
    /// Truncates the input string to a specified number of characters from the left and appends an ellipsis if the string exceeds the given length.
    /// </summary>
    /// <param name="value">The input string to truncate. Can be null.</param>
    /// <param name="count">The maximum number of characters to retain from the start of the string.</param>
    /// <returns>
    /// A string truncated to the specified length with an appended ellipsis if required.
    /// Returns an empty string if the input is null or empty.
    /// </returns>
    public static string LeftEllipsis(this string? value, int count)
    {
        if (value.IsEmpty() || count == 0)
            return "";

        if (value.Length <= count)
            return value;

        return value.Left(count - 1) + "…";
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
        if (length < 1)
            throw new ArgumentOutOfRangeException(nameof(length));

        var effectiveLength = Math.Min(value.Length, length);
        return new string(mask, effectiveLength) + value.Right(-effectiveLength);
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
        if (length < 1)
            throw new ArgumentOutOfRangeException(nameof(length));

        var effectiveLength = Math.Min(value.Length, length);
        return value.Left(-effectiveLength) + new string(mask, effectiveLength);
    }

    /// <summary>
    /// Get a substring from the middle of the text to the end. If the offset is outside
    /// of the string, an empty string will be returned.
    /// </summary>
    /// <param name="value">String to work with</param>
    /// <param name="offset">Zero-based offset</param>
    public static string Mid(this string? value, int offset)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return offset < value.Length ? value[offset..] : string.Empty;
    }

    /// <summary>
    /// Get a substring from the middle of the text. If the offset or count is outside
    /// of the string, as many characters as was found will be returned.
    /// </summary>
    /// <param name="value">String to work with</param>
    /// <param name="offset">Zero-based offset</param>
    /// <param name="count">Number of characters to extract</param>
    public static string Mid(this string? value, int offset, int count)
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
    /// Process whitespace in string according to rules. Can trim start/end of the string, replace
    /// tabs and CRLFs with spaces, and remove duplicate spaces.
    /// </summary>
    public static string NormalizeSpacing(this string? s, Spacing spacing = Spacing.ReplaceAll)
    {
        s ??= "";

        if ((spacing & Spacing.ReplaceTabs) != 0)
            s = s.Replace("\t", " ");

        if ((spacing & Spacing.ReplaceCrLfs) != 0)
            s = s
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ");

        if ((spacing & Spacing.ReplaceDoubleSpaces) != 0)
            while (s.Contains("  "))
                s = s.Replace("  ", " ");

        if ((spacing & Spacing.Trim) != 0)
            s = s.Trim();

        return s;
    }

    /// <summary>
    /// Returns null if the string IsNullOrEmpty.
    /// </summary>
    /// <param name="value">String to test.</param>
    /// <returns>Null if the string is empty, otherwise the original string.</returns>
    public static string? NullIfEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }

    /// <summary>
    /// Parse a string to a boolean. Handles truthy/falsy numbers, strings like "true/false", "yes/no", "t/f" and "y/n".
    /// If the input string is empty or cannot be parsed, it returns null.
    /// </summary>
    public static bool? ParseBoolean(this string? value)
    {
        value = value?.Trim();
        if (string.IsNullOrEmpty(value))
            return null;

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

        if (long.TryParse(value, out var longResult))
            return longResult != 0;

        return null;
    }

    /// <summary>
    /// Parse a string to a boolean. Handles truthy/falsy numbers, strings like "true/false", "yes/no", "t/f" and "y/n".
    /// If the input string is empty or cannot be parsed, it returns a default value.
    /// </summary>
    public static bool ParseBoolean(this string? value, bool defaultValue)
    {
        return ParseBoolean(value) ?? defaultValue;
    }

    /// <summary>
    /// Parse a decimal value according to NumberStyles.Number and a given culture.
    /// </summary>
    public static decimal ParseDecimal(this string? value, CultureInfo culture, decimal defaultValue = 0)
    {
        return decimal.TryParse((value ?? "").Trim(), NumberStyles.Number, culture, out var result)
            ? result
            : defaultValue;
    }

    /// <summary>
    /// Parse a decimal value according to NumberStyles.Number and the current culture.
    /// </summary>
    public static decimal ParseDecimal(this string? value, decimal defaultValue = 0)
    {
        return ParseDecimal(value, CultureInfo.CurrentCulture, defaultValue);
    }

    /// <summary>
    /// Parse a decimal value according to NumberStyles.Number and the invariant culture.
    /// </summary>
    public static decimal ParseDecimalInvariant(this string? value, decimal defaultValue = 0)
    {
        return ParseDecimal(value, CultureInfo.InvariantCulture, defaultValue);
    }

    /// <summary>
    /// Parse a double value according to NumberStyles.Number and a given culture.
    /// </summary>
    public static double ParseDouble(this string? value, CultureInfo culture, double defaultValue = 0)
    {
        return double.TryParse((value ?? "").Trim(), NumberStyles.Number, culture, out var result)
            ? result
            : defaultValue;
    }

    /// <summary>
    /// Parse a double value according to NumberStyles.Number and the current culture.
    /// </summary>
    public static double ParseDouble(this string? value, double defaultValue = 0)
    {
        return ParseDouble(value, CultureInfo.CurrentCulture, defaultValue);
    }

    /// <summary>
    /// Parse a double value according to NumberStyles.Number and the invariant culture.
    /// </summary>
    public static double ParseDoubleInvariant(this string? value, double defaultValue = 0)
    {
        return ParseDouble(value, CultureInfo.InvariantCulture, defaultValue);
    }

    /// <summary>
    /// Parse a integer value.
    /// </summary>
    public static int ParseInt(this string? value, int defaultValue = 0)
    {
        return int.TryParse((value ?? "").Trim(), out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Parse a long value.
    /// </summary>
    public static long ParseLong(this string? value, long defaultValue = 0)
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
    public static string Right(this string? value, int count)
    {
        if (count < 0)
            count = (value?.Length ?? 0) + count;

        return string.IsNullOrEmpty(value)
            ? string.Empty
            : Mid(value, value.Length - count, count);
    }

    /// <summary>
    /// Return the string with the first letter in lowercase.
    /// </summary>
    public static string? StartLowerCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length == 1
            ? value.ToLower()
            : char.ToLower(value[0]) + value.Substring(1, value.Length - 1);
    }

    /// <summary>
    /// Return the string with the first letter in uppercase.
    /// </summary>
    public static string? StartUpperCase(this string? value)
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
    public static IEnumerable<string> TrimAndFilter(this IEnumerable<string?> strings)
    {
        return strings
            .Select(x => x?.Trim())
            .Where(x => !string.IsNullOrEmpty(x))!;
    }
}
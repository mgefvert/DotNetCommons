// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

using System;
using System.Linq;

namespace DotNetCommons;

public static partial class CommonStringExtensions
{
    internal static int FindSubItemIndex(string source, char delimiter, int index, int startPosition = 0)
    {
        if (index == 0)
            return startPosition;
        
        var pos = startPosition;
        while (index > 0)
        {
            pos = source.IndexOf(delimiter, pos);
            if (pos == -1)
                return -1;
            pos++;
            index--;
        }

        return pos;
    }

    /// <summary>
    /// Adds an element to a string with sub-items separated by a delimiter, e.g.
    /// "foo.bar.abc". Adding a new "xyz" element to this, yield "foo.bar.abc.xyz".
    /// </summary>
    /// <param name="source">Source string to operate on.</param>
    /// <param name="delimiter">Delimiter character that separates sub-items.</param>
    /// <param name="value">String to add.</param>
    /// <returns>A new string with the new value added.</returns>
    public static string AddSubItem(this string? source, char delimiter, string value)
    {
        return source == null ? value : source + delimiter + value;
    }

    /// <summary>
    /// Gets a particular zero-indexed sub-item from a string of sub-items. For instance, operating
    /// on a string "foo.bar.abc.xyz", GetSubItem('.', 2) would return "abc". The operation
    /// is fairly inexpensive and optimized for efficient string processing.
    /// </summary>
    /// <param name="source">Source string to operate on.</param>
    /// <param name="delimiter">Delimiter character that separates sub-items.</param>
    /// <param name="index">Zero-based index of the sub-item.</param>
    /// <returns>The sub-item fetched from the string.</returns>
    public static string? GetSubItem(this string? source, char delimiter, int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (source == null)
            return null;

        var start = FindSubItemIndex(source, delimiter, index);
        if (start == -1)
            return null;
        
        var end = FindSubItemIndex(source, delimiter, 1, start);

        return end == -1 ? source.Substring(start) 
            : end <= start + 1 ? "" 
            : source.Substring(start, end - start - 1);
    }

    /// <summary>
    /// Returns the number of sub-items in a string separated by a delimiter. For instance,
    /// the number of sub-items in the string "foo.bar.abc.xyz" is four, if the separator
    /// is a period char. Note: An empty string is considered having 1 sub-item, whereas
    /// a null string is considered having 0.
    /// </summary>
    /// <param name="source">Source string to operate on.</param>
    /// <param name="delimiter">Delimiter character that separates sub-items.</param>
    /// <returns>Number of sub-items in the string.</returns>
    public static int GetSubItemCount(this string? source, char delimiter)
    {
        return source == null ? 0 : source.Count(c => c == delimiter) + 1;
    }

    /// <summary>
    /// Insert a sub-item into a string of sub-items separated by a delimiter. For instance,
    /// inserting "xyz" into the string "foo.bar" at position 1 (zero-based) would yield the
    /// string "foo.xyz.bar". You can also add sub-items beyond the end of the string, in which
    /// case the appropriate number of delimiters is added automatically. The operation
    /// is fairly inexpensive and optimized for efficient string processing.
    /// </summary>
    /// <param name="source">Source string to operate on.</param>
    /// <param name="delimiter">Delimiter character that separates sub-items.</param>
    /// <param name="index">Zero-based position where to insert the new sub-item.</param>
    /// <param name="value">The new value to insert.</param>
    /// <returns>A new string containing the inserted new value.</returns>
    public static string InsertSubItem(this string? source, char delimiter, int index, string value)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (source == null)
            return new string(delimiter, index) + value;
        
        if (index == 0)
            return value + "|" + source;

        var count = GetSubItemCount(source, delimiter);
        if (index >= count)
            return source + new string(delimiter, index - count + 1) + value;

        var start = FindSubItemIndex(source, delimiter, index);

        return source.Substring(0, start) + value + source.Substring(start - 1);
    }

    /// <summary>
    /// Removes a sub-item from a string of sub-items separated by a delimiter. For instance,
    /// removing element number 1 (zero-based) from the string "foo.bar.abc.xyz" would yield
    /// the string "foo.abc.xyz". Note: Removing all elements from a string will return null.
    /// The operation is fairly inexpensive and optimized for efficient string processing.
    /// </summary>
    /// <param name="source">Source string to operate on.</param>
    /// <param name="delimiter">Delimiter character that separates sub-items.</param>
    /// <param name="index">Zero-based position what sub-item to remove.</param>
    /// <returns>A new string with the given element removed.</returns>
    public static string? RemoveSubItem(this string? source, char delimiter, int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (source == null)
            return null;
        
        var count = GetSubItemCount(source, delimiter);
        if (index == 0 && count == 1)
            return null;
        if (index >= count)
            return source;

        var start = FindSubItemIndex(source, delimiter, index);
        var end = FindSubItemIndex(source, delimiter, 1, start);

        if (start == 0)
            return source.Substring(end);
        
        return end == -1
            ? source.Substring(0, start - 1)
            : source.Substring(0, start - 1) + source.Substring(end-1);
    }

    /// <summary>
    /// Sets a given sub-item to a new value, in a string of sub-items separated by a delimiter. For instance,
    /// setting element number 1 (zero-based) to "humbug", in the string "foo.bar.abc" would yield
    /// the string "foo.humbug.xyz". The new value must be not-null; to remove a sub-item, see <see cref="RemoveSubItem"/>.
    /// The operation is fairly inexpensive and optimized for efficient string processing.
    /// </summary>
    /// <param name="source">Source string to operate on.</param>
    /// <param name="delimiter">Delimiter character that separates sub-items.</param>
    /// <param name="index">Zero-based position what sub-item to set.</param>
    /// <param name="value">The new value.</param>
    /// <returns>A new string with the given element changed to the new value.</returns>
    public static string SetSubItem(this string? source, char delimiter, int index, string value)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        int start;
        source ??= "";
        
        if (index == 0)
        {
            start = FindSubItemIndex(source, delimiter, 1);
            return start == -1 ? value : value + source.Substring(start - 1);
        }

        var count = GetSubItemCount(source, delimiter);
        if (index >= count)
            return source + new string(delimiter, index - count + 1) + value;

        start = FindSubItemIndex(source, delimiter, index);
        var end = FindSubItemIndex(source, delimiter, 1, start);

        return end == -1
            ? source.Substring(0, start) + value
            : source.Substring(0, start) + value + source.Substring(end-1);
    }
}
// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

using System;
using System.Text;

namespace DotNetCommons;

public enum CaseType
{
    CamelCase,
    KebabCase,
    PascalCase,
    SentenceCase,
    SnakeCase,
}

public static partial class CommonStringExtensions
{
    /// <summary>
    /// Transform a string into a specific case.
    /// </summary>
    public static string ToCase(this string value, CaseType caseType)
    {
        return caseType switch
        {
            CaseType.CamelCase => ToCamelCase(value, false),
            CaseType.KebabCase => ToSeparatorCase(value, '-'),
            CaseType.PascalCase => ToCamelCase(value, true),
            CaseType.SentenceCase => ToSeparatorCase(value, ' '),
            CaseType.SnakeCase => ToSeparatorCase(value, '_'),
            _ => throw new ArgumentOutOfRangeException(nameof(caseType), caseType, null)
        };
    }

    private static string ToSeparatorCase(string value, char separator)
    {
        var search = new string(separator, 2);
        var replace = separator.ToString();

        var sb = new StringBuilder(value.Length + value.Length / 4);
        var lastUpper = false;
        foreach (var c in value)
        {
            if (char.IsLower(c) || char.IsDigit(c))
            {
                sb.Append(c);
                lastUpper = false;
            }
            else if (char.IsUpper(c))
            {
                if (!lastUpper && sb.Length > 0)
                    sb.Append(separator);
                sb.Append(char.ToLower(c));
                lastUpper = true;
            }
            else if (sb.Length > 0 && sb[^1] != separator)
                sb.Append(separator);
        }

        var result = sb.ToString();
        while (result.Contains(search))
            result = result.Replace(search, replace);

        return result.Trim(separator).ToLower();
    }

    private static string ToCamelCase(string value, bool startUpper)
    {
        value = ToSeparatorCase(value, ' ');

        var result = new StringBuilder(value.Length);
        var nextUpper = false;
        foreach (var c in value)
        {
            if (result.Length == 0)
            {
                if (char.IsLetter(c))
                    result.Append(startUpper ? char.ToUpper(c) : char.ToLower(c));
            }
            else if (c == ' ')
                nextUpper = true;
            else if (nextUpper)
            {
                result.Append(char.ToUpper(c));
                nextUpper = false;
            }
            else
                result.Append(c);
        }

        return result.ToString();
    }
}

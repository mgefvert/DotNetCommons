// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

using System.Text;
using System.Text.RegularExpressions;

namespace DotNetCommons;

public enum CaseType
{
    /// <summary>thisIsCamelCase</summary>
    CamelCase,
    /// <summary>this-is-kebab-case</summary>
    KebabCase,
    /// <summary>ThisIsPascalCase</summary>
    PascalCase,
    /// <summary>this is sentence case</summary>
    SentenceCase,
    /// <summary>this_is_snake_case</summary>
    SnakeCase,
}

public static partial class CommonStringExtensions
{
    private static readonly Regex ReplaceCaseRegex = new(@"[^a-zA-Z0-9]");

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
        var singleSeparator = separator.ToString();
        var doubleSeparators = new string(separator, 2);

        // Replace anything not a letter or a digit with the given separator
        value = ReplaceCaseRegex.Replace(value, singleSeparator);

        // If a separator is in the text, we can assume that words will be separated by the separator.
        // This comes into play when we have things like multiple uppercase letters; are they to be
        // treated as a single word or a sequence of words? "IAmAString" should be "i am a string", not
        // "i am astring"; but "I have a KVM switch" should be "i have a kvm switch", not "i have a k v m switch".
        var separatorFound = value.Contains(singleSeparator);

        // Allocate space for the new string plus some leeway
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
                if ((!lastUpper || !separatorFound) && sb.Length > 0)
                    sb.Append(separator);
                sb.Append(char.ToLower(c));
                lastUpper = true;
            }
            else if (sb.Length > 0 && sb[^1] != separator)
                sb.Append(separator);
        }

        // Replace any consecutive separators with a single separator
        var result = sb.ToString();
        while (result.Contains(doubleSeparators))
            result = result.Replace(doubleSeparators, singleSeparator);

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

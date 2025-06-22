using System.Text.RegularExpressions;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.Text;

public static class Wildcards
{
    /// <summary>
    /// Match a filename to a file pattern, using an internal regex transformation.
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="match"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static bool Match(string pattern, string match, bool ignoreCase = false)
    {
        return ToRegex(pattern, ignoreCase).IsMatch(match);
    }

    /// <summary>
    /// Turn a wildcard file pattern into a regex, handling ??? and * parameters.
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static Regex ToRegex(string pattern, bool ignoreCase = false)
    {
        return new Regex("^" + Regex.Escape(pattern).Replace("\\?", ".").Replace("\\*", ".*") + "$", 
            ignoreCase ? RegexOptions.Singleline | RegexOptions.IgnoreCase : RegexOptions.Singleline);
    }
}
using System;
using System.Text.RegularExpressions;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.Text
{
    public static class Wildcards
    {
        public static bool Match(string pattern, string match, bool ignoreCase = false)
        {
            return ToRegex(pattern, ignoreCase).IsMatch(match);
        }

        public static Regex ToRegex(string pattern, bool ignoreCase = false)
        {
            return new Regex("^" + Regex.Escape(pattern).Replace("\\?", ".").Replace("\\*", ".*") + "$", 
                ignoreCase ? RegexOptions.Singleline | RegexOptions.IgnoreCase : RegexOptions.Singleline);
        }
    }
}

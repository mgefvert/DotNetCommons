using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotNetCommons.WinForms
{
    public sealed class NaturalStringComparer : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        public int Compare(string a, string b)
        {
            return StrCmpLogicalW(a, b);
        }
    }
}

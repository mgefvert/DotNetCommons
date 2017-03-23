using System;
using System.IO;
using System.Security.Principal;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons
{
    public static class FileInfoExtensions
    {
        public static string GetOwner(this FileInfo info)
        {
            try
            {
                return File.GetAccessControl(info.FullName)?.GetOwner(typeof(NTAccount))?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}

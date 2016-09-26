using System;
using System.IO;
using System.Security.Principal;

namespace CommonNetTools
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

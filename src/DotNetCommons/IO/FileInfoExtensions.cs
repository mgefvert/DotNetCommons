using System;
using System.IO;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO
{
    public static class FileInfoExtensions
    {
        public static void Touch(this FileInfo file)
        {
            if (file.Exists)
                using (file.AppendText()) {}
            else
                using (file.Create()) {}
        }
    }
}

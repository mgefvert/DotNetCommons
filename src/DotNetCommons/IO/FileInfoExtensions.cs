using System;
using System.IO;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO
{
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Create a new file, or update the last written timestamp of it.
        /// </summary>
        /// <param name="file"></param>
        public static void Touch(this FileInfo file)
        {
            if (file.Exists)
                using (file.AppendText()) {}
            else
                using (file.Create()) {}
        }
    }
}

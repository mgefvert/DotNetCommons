using System;
using System.IO;

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

using System.IO;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonFileInfoExtensions
{
    /// <summary>
    /// Create a new file, or update the last written timestamp of it.
    /// </summary>
    /// <param name="file"></param>
    public static void Touch(this FileInfo file)
    {
        if (file.Exists)
            using (file.AppendText()) { }
        else
            using (file.Create()) { }
    }
}
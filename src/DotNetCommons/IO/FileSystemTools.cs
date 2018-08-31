using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetCommons.Collections;

namespace DotNetCommons.IO
{
    public static class FileSystemTools
    {
        private static IEnumerable<FileSystemInfo> InternalFind(string path, List<string> groups)
        {
            if (!Directory.Exists(path) || !groups.Any())
                return new FileInfo[0];

            var group = groups.ExtractFirst();

            if (!group.Contains("*") && !group.Contains("?") && groups.Any())
                return InternalFind(path + "\\" + group, groups);

            if (!groups.Any())
                return Directory.EnumerateFileSystemEntries(path, group)
                    .Select(x => Directory.Exists(x) ? (FileSystemInfo)new DirectoryInfo(x) : new FileInfo(x));

            var result = new List<FileSystemInfo>();
            foreach (var entry in Directory.EnumerateDirectories(path, group))
                result.AddRange(InternalFind(path + "\\" + Path.GetFileName(entry), groups.ToList()));

            return result;
        }

        public static IEnumerable<FileSystemInfo> Glob(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                pattern = "*";

            var cwd = new Uri(Directory.GetCurrentDirectory() + "\\");
            var groups = new Uri(cwd, pattern).LocalPath.Split('\\').ToList();
            var path = groups.ExtractFirst();

            return InternalFind(path, groups);
        }
    }
}

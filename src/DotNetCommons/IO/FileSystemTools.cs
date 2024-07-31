// ReSharper disable UnusedMember.Global

namespace DotNetCommons.IO;

public static class FileSystemTools
{
    private static IEnumerable<FileSystemInfo> InternalFind(string path, List<string> groups)
    {
        if (!Directory.Exists(path) || !groups.Any())
            return Array.Empty<FileInfo>();

        var group = groups.ExtractFirst();

        if (!group.Contains('*') && !group.Contains('?') && groups.Any())
            return InternalFind(path + Path.DirectorySeparatorChar + group, groups);

        if (!groups.Any())
            return Directory.EnumerateFileSystemEntries(path, group)
                .Select(x => Directory.Exists(x) ? (FileSystemInfo)new DirectoryInfo(x) : new FileInfo(x));

        var result = new List<FileSystemInfo>();
        foreach (var entry in Directory.EnumerateDirectories(path, group))
            result.AddRange(InternalFind(path + Path.DirectorySeparatorChar + Path.GetFileName(entry), groups.ToList()));

        return result;
    }

    /// <summary>
    /// A simple filename expansion method that can understand paths like a\*\b and return
    /// a sequence of files found.
    /// </summary>
    public static IEnumerable<FileSystemInfo> Glob(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            pattern = "*";

        var cwd = new Uri(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
        var groups = new Uri(cwd, pattern).LocalPath.Split(Path.DirectorySeparatorChar).ToList();
        var path = groups.ExtractFirst();

        return InternalFind(path, groups);
    }
}
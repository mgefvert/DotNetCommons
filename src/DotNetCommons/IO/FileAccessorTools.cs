// ReSharper disable UnusedMember.Global

using System.Text.RegularExpressions;
using DotNetCommons.Text;

namespace DotNetCommons.IO;

public static class FileAccessorTools
{
    private static readonly Regex DrivePath = new(@"^[a-zA-Z]:[/\\]", RegexOptions.Singleline); 
    
    private static IEnumerable<IFileItem> InternalFind(IFileAccessor accessor, string pattern, IFileItem current)
    {
        var part = pattern.GetSubItem(accessor.DirectorySeparator, 0);
        pattern = pattern.RemoveSubItem(accessor.DirectorySeparator, 0)!;

        if (part == ".")
        {
            foreach (var item in InternalFind(accessor, pattern, current))
                yield return item;
            yield break;
        }
        
        if (part == "..")
        {
            foreach (var item in InternalFind(accessor, pattern, current.Parent ?? throw new DirectoryNotFoundException()))
                yield return item;
            yield break;
        }
        
        var regex     = Wildcards.ToRegex(part!, true);
        var directory = !pattern.IsEmpty();

        if (directory)
        {
            // Match on directories
            foreach (var match in current.ListFiles().Where(x => x.Directory && regex.IsMatch(x.Name)))
            {
                foreach (var item in InternalFind(accessor, pattern, match))
                    yield return item;
            }
        }
        else
        {
            // Match on files, there's nothing left in pattern so this has to be a file
            foreach (var item in current.ListFiles().Where(x => !x.Directory && regex.IsMatch(x.Name)))
                yield return item;
        }
    }

    /// <summary>
    /// A simple filename expansion method that can understand paths like a\*\b and return
    /// a sequence of files found.
    /// </summary>
    public static IEnumerable<IFileItem> Glob(this IFileAccessor accessor, string pattern)
    {
        pattern = pattern
            .Replace('/', accessor.DirectorySeparator)
            .Replace('\\', accessor.DirectorySeparator);

        var twice = new string(accessor.DirectorySeparator, 2);
        while (pattern.Contains(twice))
            pattern = pattern.Replace(twice, accessor.DirectorySeparator.ToString());
            
        if (pattern.IsEmpty() || pattern.EndsWith(accessor.DirectorySeparator))
            pattern += "*";

        var current = accessor.CurrentItem;
            
        // Does this start with a drive letter?
        if (DrivePath.IsMatch(pattern))
        {
            var root = pattern.GetSubItem(accessor.DirectorySeparator, 0) + accessor.DirectorySeparator;
            pattern = pattern.RemoveSubItem(accessor.DirectorySeparator, 0)!;
            current = accessor.GetDirectory(root, false) ?? throw new DirectoryNotFoundException();
        }
        
        // Is this path rooted?
        if (pattern.StartsWith(accessor.DirectorySeparator))
        {
            current = accessor.GetDirectory(accessor.DirectorySeparator.ToString(), false) ?? throw new DirectoryNotFoundException();
            pattern = pattern[1..];
        }

        foreach (var item in InternalFind(accessor, pattern, current))
            yield return item;
    }
}
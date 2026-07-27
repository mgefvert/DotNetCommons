using DotNetCommons.Commands;
using DotNetCommons.Sys;

namespace commons.Commands;

public class DevCleanArgs
{
    [CommandLineOption('d', "directory", "Directory to clean, default '.'")]
    public string Directory { get; set; } = ".";

    [CommandLineOption("dry-run", "Don't actually delete anything")]
    public bool DryRun { get; set; }
}

[CommandAction(["dev", "clean"], "Clean up development folders from a directory", [])]
public class DevCleanCommand : CommandAction<DevCleanArgs>
{
    private static readonly HashSet<string> Ignore = new(StringComparer.InvariantCultureIgnoreCase) { "node_modules" };
    private static readonly HashSet<string> Include = new(StringComparer.InvariantCultureIgnoreCase) { "bin", "obj", "TestResults" };

    public override int Execute()
    {
        var directory = new DirectoryInfo(Args.Directory);
        if (!directory.Exists)
        {
            Console.WriteLine($"Directory '{directory.FullName}' does not exist.");
            return 1;
        }

        Console.WriteLine(Args.DryRun ? "Candidates for deletion:" : "Deleting folders:");

        var n = CleanDirectory(directory);

        Console.WriteLine(n > 0 ? $"Deleted {n} folders." : "No folders deleted.");

        return 0;
    }

    private int CleanDirectory(DirectoryInfo directory)
    {
        var dirs  = directory.EnumerateDirectories().ToList();
        var count = 0;

        foreach (var dir in dirs)
        {
            if (Include.Contains(dir.Name))
            {
                Console.WriteLine(dir.FullName);
                if (!Args.DryRun)
                {
                    dir.Delete(true);
                    count++;
                }
            }
            else if (!dir.Name.StartsWith('.') && !Ignore.Contains(dir.Name))
                count += CleanDirectory(dir);
        }

        return count;
    }
}
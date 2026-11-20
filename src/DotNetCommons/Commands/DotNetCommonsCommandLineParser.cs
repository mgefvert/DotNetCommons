using DotNetCommons.Sys;

namespace DotNetCommons.Commands;

/// <summary>
/// Helper class that uses the CommandLine classes to parse command line arguments.
/// </summary>
public class DotNetCommonsCommandLineParser : ICommandLineParser
{
    private static readonly CommandLineSettings Settings = new()
    {
        DisplayHelpOnEmpty = false
    };

    public void DisplayHelpFor(TextWriter output, Type optionType)
    {
        output.WriteLine(CommandLine.GetFormattedHelpText(optionType));
    }

    public object Parse(Type optionType, string[] args)
    {
        return CommandLine.Parse(optionType, Settings, args);
    }
}

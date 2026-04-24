using DotNetCommons.Sys;

namespace DotNetCommons.Commands;

/// <summary>
/// Helper class that uses the CommandLine classes to parse command line arguments.
/// </summary>
public class DotNetCommonsCommandLineParser : ICommandLineParser
{
    public void DisplayHelpFor(TextWriter output, Type optionType)
    {
        output.WriteLine(CommandLine.GetFormattedHelpText(optionType));
    }

    public object Parse(Type optionType, string[] args)
    {
        CommandLine.DisplayHelpOnEmpty = false;
        return CommandLine.Parse(optionType, args);
    }
}
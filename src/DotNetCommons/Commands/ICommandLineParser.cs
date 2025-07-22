namespace DotNetCommons.Commands;

/// <summary>
/// Abstraction interface that enables CommandActionRegistry to use different command line parsers.
/// </summary>
public interface ICommandLineParser
{
    /// <summary>
    /// Displays help information for the specified command line option type, formatted appropriately for the output.
    /// </summary>
    /// <param name="output">The text writer to which the help information should be written.</param>
    /// <param name="optionType">The type representing the command line option for which help information is required.</param>
    void DisplayHelpFor(TextWriter output, Type optionType);

    /// <summary>
    /// Parses the specified command line arguments into an object of the given option type.
    /// </summary>
    /// <param name="optionType">The type representing the command line options to be parsed.</param>
    /// <param name="args">An array of command line arguments to parse.</param>
    object Parse(Type optionType, string[] args);
}
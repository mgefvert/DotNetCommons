namespace DotNetCommons.Commands;

/// <summary>
/// Required attribute that provides metadata for CommandActions.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CommandActionAttribute : Attribute
{
    /// <summary>
    /// Gets the route associated with the command action. Routes define the command-line paths or keywords used to trigger a specific
    /// command action, e.g. the command "container create" would have ["container", "create"] as its route.
    /// </summary>
    public string[] Route { get; }

    /// <summary>
    /// One-line description of the command action, suitable for displaying in an overview of all commands.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the detailed help text for the command action. Help text is provided as an array of strings,
    /// where each entry represents a paragraph of information intended to guide the user in using the command.
    /// This content may include usage details, examples, and additional notes.
    /// </summary>
    public string[] HelpText { get; }

    /// <summary>
    /// Represents an attribute used to annotate a class with metadata defining a command action.
    /// It provides routing information and a description for command-line based actions.
    /// </summary>
    /// <param name="route">The route associated with the command action</param>
    /// <param name="description">One-line description of the command action, suitable for displaying in an overview of all commands.</param>
    /// <param name="helpText">Detailed help for this command, provided as an array of paragraphs. Each paragraph will be formatted
    ///     according to the width of the console.</param>
    public CommandActionAttribute(string[] route, string description, string[] helpText)
    {
        Route       = route;
        Description = description;
        HelpText    = helpText;
    }
}
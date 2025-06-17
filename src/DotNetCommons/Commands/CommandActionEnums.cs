namespace DotNetCommons.Commands;

[Flags]
public enum CommandActionOptions
{
    None = 0,
    Default = AllowHelpVerb | ShowHelpOnMultipleMatches | ShowHelpOnNoArgs,
    
    /// <summary>
    /// Allows the user to write "help" before or after the command to trigger help.
    /// </summary>
    AllowHelpVerb = 1,
    
    /// <summary>
    /// Automatically handle multiple matches by listing the commands.
    /// </summary>
    ShowHelpOnMultipleMatches = 2,

    /// <summary>
    /// Show help when no arguments are provided.
    /// </summary>
    ShowHelpOnNoArgs = 4
}
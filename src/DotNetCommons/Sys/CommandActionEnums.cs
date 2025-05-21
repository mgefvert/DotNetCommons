namespace DotNetCommons.Sys;

[Flags]
public enum CommandActionOptions
{
    None = 0,
    Default = AllowHelp | HandleMultipleMatches,
    
    /// <summary>
    /// Allows the user to write "help" before or after the command to trigger help.
    /// </summary>
    AllowHelp = 1,
    
    /// <summary>
    /// Automatically handle multiple matches by listing the commands.
    /// </summary>
    HandleMultipleMatches = 2,
}
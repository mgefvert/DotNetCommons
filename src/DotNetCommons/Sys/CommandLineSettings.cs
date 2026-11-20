namespace DotNetCommons.Sys;

/// Settings for a single command-line parse.
public sealed record CommandLineSettings
{
    /// Handle -aop as -a, -o, and -p.
    public bool MultipleShortOptions { get; init; } = true;

    /// Request help when the argument list is empty.
    public bool DisplayHelpOnEmpty { get; init; } = true;
}

namespace DotNetCommons.Commands;

/// <summary>
/// Defines an alternative command route that is hidden from command overviews.
/// Aliases must be invoked using their full route.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class CommandActionAliasAttribute : Attribute
{
    /// Gets the alternative route used to invoke the command action.
    public string[] Route { get; }

    /// Gets or sets whether invoking this alias warns the user to use the canonical command route instead.
    public bool Deprecated { get; set; }

    public CommandActionAliasAttribute(string[] route)
    {
        Route = route;
    }
}

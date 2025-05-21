namespace DotNetCommons.Sys;

[AttributeUsage(AttributeTargets.Class)]
public class CommandActionAttribute : Attribute
{
    public string[] Route { get; }
    public string Description { get; }

    public CommandActionAttribute(string[] route, string description)
    {
        Route       = route;
        Description = description;
    }
}
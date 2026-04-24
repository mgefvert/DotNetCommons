namespace DotNetCommons.Commands;

/// <summary>
/// Represents a data structure utilized for executing command actions within a command registry.
/// </summary>
public class Invocation
{
    public Type Action { get; }
    public bool ContinueOnError { get; }
    public object? Options { get; }

    public CommandActionRegistry? Registry { get; set; }

    public Invocation(Type action, object? options, bool continueOnError)
    {
        Action = action;
        Options = options;
        ContinueOnError = continueOnError;
    }
}
using System.Reflection;

namespace DotNetCommons.Commands;

/// <summary>
/// Represents an abstract base class for command actions that do not require a specific argument model.
/// This class derives from CommandAction with a generic argument of <see cref="System.Object"/>
/// and sets up a default argument instance.
/// </summary>
public abstract class CommandAction
{
    public string[] GetRoute()
    {
        var attribute = GetType().GetCustomAttribute<CommandActionAttribute>()
                        ?? throw new InvalidOperationException($"{GetType().Name} has no CommandActionAttribute");

        return attribute.Route;
    }

    /// <summary>
    /// The command action registry associated with the current command action. This property
    /// allows interaction with the system of registering, resolving, and executing commands.
    /// </summary>
    public CommandActionRegistry Registry { get; set; } = null!;

    /// <summary>
    /// Executes the associated command action.
    /// </summary>
    /// <returns>
    /// An integer representing the result of the executed command, where 0 is success.
    /// </returns>
    public abstract int Execute();
}

/// <summary>
/// The base implementation of ICommandAction with support for generic argument models. This class provides a foundation for defining
/// command actions where arguments are dynamically supplied and executed based on specific requirements.
/// </summary>
/// <typeparam name="TArgs">The type of arguments associated with the command action.
/// It must be a reference type and have a parameterless constructor.</typeparam>
public abstract class CommandAction<TArgs> : CommandAction
    where TArgs : class, new()
{
    /// <summary>
    /// Command-line arguments associated with the command action.
    /// </summary>
    public TArgs Args { get; set; } = null!;
}

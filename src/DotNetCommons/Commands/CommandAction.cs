namespace DotNetCommons.Commands;

/// <summary>
/// Represents an interface for defining a command action that can be executed.
/// This interface serves as a base contract for implementing specific command actions.
/// </summary>
public interface ICommandAction
{
    /// <summary>
    /// Executes the command action.
    /// </summary>
    /// <returns>
    /// 0 on success, otherwise an error.
    /// </returns>
    int Execute();
}

/// <summary>
/// The base implementation of ICommandAction with support for generic argument models. This class provides a foundation for defining
/// command actions where arguments are dynamically supplied and executed based on specific requirements.
/// </summary>
/// <typeparam name="TArgs">The type of arguments associated with the command action.
/// It must be a reference type and have a parameterless constructor.</typeparam>
public abstract class CommandAction<TArgs> : ICommandAction
    where TArgs : class, new()
{
    /// <summary>
    /// Command-line arguments associated with the command action.
    /// </summary>
    public TArgs Args { get; set; } = null!;

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
/// Represents an abstract base class for command actions that do not require a specific argument model.
/// This class derives from CommandAction with a generic argument of <see cref="System.Object"/>
/// and sets up a default argument instance.
/// </summary>
public abstract class CommandAction : CommandAction<object>
{
    public CommandAction()
    {
        Args = new object();
    }
}

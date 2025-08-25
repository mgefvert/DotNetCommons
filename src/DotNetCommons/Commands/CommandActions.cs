namespace DotNetCommons.Commands;

/// Represents an abstract base class for command actions that do not require a specific argument model.
/// This class derives from CommandAction with a generic argument of <see cref="System.Object"/>
/// and sets up a default argument instance.
public abstract class CommandAction
{
    /// The command action registry associated with the current command action. This property
    /// allows interaction with the system of registering, resolving, and executing commands.
    public CommandActionRegistry Registry { get; set; } = null!;

    /// Represents the global scope across all the scheduled jobs. This provides scoped services that can run across
    /// several different jobs, and yet doesn't require a singleton.
    public IServiceProvider GlobalScope { get; set; } = null!;

    /// <summary>
    /// Executes the associated asynchronous command action.
    /// </summary>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>0 on success.</returns>
    /// <remarks>
    /// This method is the default implementation for ExecuteAsync and simply calls <see cref="Execute"/>. It can be overriden in
    /// a derived class to provide async command execution.
    /// </remarks>
    public virtual Task<int> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(Execute());
    }

    /// <summary>
    /// Executes the associated command action.
    /// </summary>
    /// <returns>
    /// An integer representing the result of the executed command, where 0 is success.
    /// </returns>
    /// <remarks>
    /// This method is the default implementation for Execute and simply returns 0. Either this method or <see cref="ExecuteAsync"/>
    /// must be overriden in a derived class to provide actual command execution.
    /// </remarks>
    public virtual int Execute()
    {
        return 0;
    }
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

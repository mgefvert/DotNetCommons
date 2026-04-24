namespace DotNetCommons.Synchronization;

/// <summary>
/// Provides an atomic, thread-safe mechanism for accessing and managing a single instance of a reference type.
/// Useful in circumstances where heavy objects take time to construct, get rebuilt, and need to be swapped out in the
/// middle of a process, without requiring the entire process to be restarted.
/// </summary>
/// <remarks>
/// This is most useful in the case of providing access to heavy objects in Dependency Injection, where you can register
/// an Accessor{HeavyObject} as a singleton, and dynamically swap out the object wrapped in the Accessor.
/// </remarks>
public class Accessor<T> where T : class
{
    private readonly object _sync = new();

    private volatile T? _value;
    private TaskCompletionSource<T> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// Gets a value that indicates whether the <see cref="Accessor{T}"/> currently contains a non-null value.
    public bool Exists => _value != null;

    /// Provides an atomic, thread-safe mechanism for accessing and managing a single instance of a reference type.
    /// This initializes an empty Accessor; the actual object might be provided later.
    public Accessor()
    {
    }

    /// Provides an atomic, thread-safe mechanism for accessing and managing a single instance of a reference type.
    /// The class allows initialization of the instance during construction or through later modifications.
    public Accessor(T value)
    {
        Replace(value);
    }

    /// Clears the current value stored in the accessor and resets the internal state.
    /// This operation will remove the existing value, if any, and replace the internal TaskCompletionSource
    /// to ensure pending and future asynchronous `GetAsync` calls are reset and can wait for a new value.
    /// The method is thread-safe, ensuring proper operation in concurrent environments.
    public void Clear()
    {
        lock (_sync)
        {
            _value = null;
            _ready = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    /// <summary>
    /// Asynchronously retrieves the instance of the reference type managed by the Accessor within the specified timeout period.
    /// If the instance is not available within the timeout duration, a <see cref="TaskCanceledException"/> is thrown.
    /// </summary>
    /// <param name="timeout">The maximum duration to wait for the instance to become available.</param>
    /// <returns>A task that represents the asynchronous operation, containing the managed instance of the reference type.</returns>
    public Task<T> GetAsync(TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        return GetAsync(cts.Token);
    }

    /// <summary>
    /// Asynchronously retrieves the instance of the reference type managed by the Accessor while watching a CancellationToken.
    /// If the instance is not available it will wait until a value becomes available or the task is canceled.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to watch.</param>
    /// <returns>A task that represents the asynchronous operation, containing the managed instance of the reference type.</returns>
    public async Task<T> GetAsync(CancellationToken cancellationToken = default)
    {
        var value = _value;
        if (value != null)
            return value;

        Task<T> waitTask;
        lock (_sync)
        {
            value = _value;
            if (value != null)
                return value;

            waitTask = _ready.Task;
        }

        return await waitTask.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously retrieves the instance of the reference type managed by the Accessor within the specified timeout period.
    /// If the instance is not available within the timeout duration, no exception is thrown but null is returned instead.
    /// </summary>
    /// <param name="timeout">The maximum duration to wait for the instance to become available.</param>
    /// <returns>A task that represents the asynchronous operation, containing the managed instance of the reference type.</returns>
    public async Task<T?> GetOrDefaultAsync(TimeSpan timeout)
    {
        try
        {
            return await GetAsync(timeout);
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    /// Retrieves the current instance of the encapsulated reference type in a thread-safe manner.
    /// If no object has been encapsulated, this throws an exception.
    public T GetOrThrow()
    {
        return _value ?? throw new InvalidOperationException($"Accessor for {typeof(T).Name} has not been initialized.");
    }

    /// Replaces the current value held by the Accessor with a new instance.
    /// This method is thread-safe and ensures that any waiting tasks are notified of the new value.
    public void Replace(T value)
    {
        lock (_sync)
        {
            _value = value;
            _ready.TrySetResult(value);
        }
    }

    /// Retrieves the current instance of the encapsulated reference type in a thread-safe manner.
    /// If no object has been encapsulated, this returns null.
    public T? TryGet()
    {
        return _value;
    }
}

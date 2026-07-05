namespace DotNetCommons.Synchronization;

/// <summary>
/// Provides a simple in-process cache for typed objects.
/// </summary>
public class AccessCache
{
    private readonly Lock _sync = new();
    private readonly TimeSpan _expiration;

    // Each cached type gets its own entry. This lets a single AccessCache instance replace several old AccessCache<T>
    // registrations while keeping each type's value, expiry, and refresh task separate.
    private readonly Dictionary<Type, ICacheEntry> _entries = [];

    /// <summary>
    /// Creates a new cache with the specified expiration.
    /// </summary>
    /// <param name="expiration">Expiration value. The default is 60 seconds.</param>
    public AccessCache(TimeSpan? expiration = null)
    {
        _expiration = expiration ?? TimeSpan.FromSeconds(60);
    }

    /// <summary>
    /// Clears all cached values and pending cache entries.
    /// </summary>
    public void Clear()
    {
        lock (_sync)
        {
            _entries.Clear();
        }
    }

    /// <summary>
    /// Clears the cached value and pending cache entry for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of cached value to clear.</typeparam>
    public void Clear<T>()
    {
        lock (_sync)
        {
            _entries.Remove(typeof(T));
        }
    }

    /// <summary>
    /// Expires the cached value for <typeparamref name="T"/> so that next time it is accessed, it will be replaced before it is
    /// returned.
    /// </summary>
    /// <typeparam name="T">The type of cached value to expire.</typeparam>
    public void Expire<T>()
    {
        lock (_sync)
        {
            if (_entries.TryGetValue(typeof(T), out var entry))
                entry.Expires = DateTimeOffset.MinValue;
        }
    }

    /// <summary>
    /// Expires all cached values so that each value will be replaced before it is returned the next time it is accessed.
    /// </summary>
    public void ExpireAll()
    {
        lock (_sync)
        {
            foreach (var entry in _entries.Values)
                entry.Expires = DateTimeOffset.MinValue;
        }
    }

    /// <summary>
    /// Gets the cached value for <typeparamref name="T"/>, or creates a new one if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of cached value to get or create.</typeparam>
    /// <param name="factory">A function that creates the value when the cache is empty or expired.</param>
    /// <param name="cancellationToken">A token used to cancel waiting for the first value to be created.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is the cached value. If no cached value exists or the
    /// cached value has expired, waits for the value to be created or replaced.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
    public async Task<T> GetOrReplaceAsync<T>(Func<Task<T>> factory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Task<T>? waitTask = null;
        TaskCompletionSource<T>? refreshSource = null;
        CacheEntry<T>? entryToRefresh = null;
        T? cachedValue = default;
        var returnCachedValue = false;

        // Decide what needs to happen while holding the lock, then start or await work after releasing it. This keeps
        // user-supplied factory code outside the critical section.
        lock (_sync)
        {
            var entry = GetEntry<T>();

            if (!entry.HasValue)
            {
                // There is no safe value to return yet. The first caller reserves the refresh; concurrent callers reuse the same
                // task and wait for it instead of running their own factory calls.
                if (entry.RefreshTask == null)
                {
                    refreshSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                    entry.RefreshTask = refreshSource.Task;
                    entryToRefresh = entry;
                }

                waitTask = entry.RefreshTask;
            }
            else if (entry.Expires <= DateTimeOffset.UtcNow)
            {
                // The old value is no longer valid once it has expired. Reserve one refresh task and make all callers wait for
                // that replacement instead of returning stale data.
                if (entry.RefreshTask == null)
                {
                    refreshSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
                    entry.RefreshTask = refreshSource.Task;
                    entryToRefresh = entry;
                }

                waitTask = entry.RefreshTask;
            }
            else
            {
                // The cached value is fresh, so no factory work is needed.
                cachedValue = entry.Value;
                returnCachedValue = true;
            }
        }

        if (refreshSource != null)
            StartRefresh(entryToRefresh!, refreshSource, factory);

        if (returnCachedValue)
            return cachedValue!;

        // Empty and expired cache paths reach this await. Cancellation only cancels this caller's wait; the shared refresh
        // continues for other callers.
        return await waitTask!.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private CacheEntry<T> GetEntry<T>()
    {
        var type = typeof(T);

        if (!_entries.TryGetValue(type, out var entry))
        {
            entry = new CacheEntry<T>();
            _entries.Add(type, entry);
        }

        return (CacheEntry<T>)entry;
    }

    private void StartRefresh<T>(
        CacheEntry<T> entry,
        TaskCompletionSource<T> refreshSource,
        Func<Task<T>> factory)
    {
        // Task.Run starts after the caller has left the lock, so the factory cannot block other callers from reading the cache
        // state. The refresh is intentionally independent from any one caller's CancellationToken because multiple callers can
        // be waiting on the same refresh task.
        _ = Task.Run(async () =>
        {
            try
            {
                var value = await factory().ConfigureAwait(false);

                lock (_sync)
                {
                    // Publish the new value atomically with its expiration timestamp, then clear RefreshTask so a later expiry
                    // can start a new refresh.
                    entry.Value = value;
                    entry.HasValue = true;
                    entry.Expires = DateTimeOffset.UtcNow.Add(_expiration);

                    if (ReferenceEquals(entry.RefreshTask, refreshSource.Task))
                        entry.RefreshTask = null;
                }

                refreshSource.TrySetResult(value);
            }
            catch (Exception ex)
            {
                lock (_sync)
                {
                    // A failed refresh should not permanently block future attempts. If a stale value existed, callers keep
                    // using it until a later refresh succeeds.
                    if (ReferenceEquals(entry.RefreshTask, refreshSource.Task))
                        entry.RefreshTask = null;
                }

                refreshSource.TrySetException(ex);
            }
        });
    }

    private interface ICacheEntry
    {
        DateTimeOffset Expires { get; set; }
    }

    private sealed class CacheEntry<T> : ICacheEntry
    {
        public bool HasValue { get; set; }
        public T? Value { get; set; }
        public DateTimeOffset Expires { get; set; } = DateTimeOffset.MinValue;
        public Task<T>? RefreshTask { get; set; }
    }
}

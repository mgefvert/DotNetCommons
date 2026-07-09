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
    public void ClearAll()
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
    public void Clear<T>() where T : class
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
    public void Expire<T>() where T : class
    {
        lock (_sync)
        {
            if (_entries.TryGetValue(typeof(T), out var entry))
                entry.Expires = DateTime.MinValue;
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
                entry.Expires = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Gets the cached value for <typeparamref name="T"/>, or creates a new one if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of cached value to get or create.</typeparam>
    /// <param name="factory">A function that creates the value when the cache is empty or expired.</param>
    /// <param name="ct">A token used to cancel waiting for the first value to be created.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is the cached value. If no cached value exists or the
    /// cached value has expired, waits for the value to be created or replaced.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
    public async Task<T> GetOrReplaceAsync<T>(Func<Task<T>> factory, CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        var entry = GetEntry<T>();

        while (!ct.IsCancellationRequested)
        {
            lock (_sync)
            {
                // First of all, check for expiration or if we don't have a value
                if ((entry.Value == null || entry.Expires <= DateTime.UtcNow) && entry.Updating == false)
                {
                    entry.Updating = true;
                    goto update;
                }

                // Do we have a previous value to return?
                if (entry.Value != null)
                    return entry.Value!;
            }

            // We're waiting for a value to appear. This only happens the first time a value is generated.
            await Task.Delay(10, ct).ConfigureAwait(false);
        }

        throw new TaskCanceledException();

        update:

        try
        {
            var newValue = await factory().ConfigureAwait(false);
            lock (_sync)
            {
                entry.Expires  = DateTime.UtcNow.Add(_expiration);
                entry.Updating = false;
                entry.Value    = newValue;
            }
            return newValue;
        }
        catch
        {
            lock (_sync)
            {
                entry.Updating = false;
            }
            throw;
        }
    }

    private CacheEntry<T> GetEntry<T>() where T : class
    {
        var type = typeof(T);

        lock (_sync)
        {
            if (!_entries.TryGetValue(type, out var entry))
                _entries.Add(type, entry = new CacheEntry<T>());

            return (CacheEntry<T>)entry;
        }
    }

    private interface ICacheEntry
    {
        DateTime Expires { get; set; }
    }

    private sealed class CacheEntry<T> : ICacheEntry where T : class
    {
        public bool Updating { get; set; }
        public T? Value { get; set; }
        public DateTime Expires { get; set; } = DateTime.MinValue;

        public override string ToString()
        {
            return $"Value={Value}; Updating={Updating}; Expires={Expires:s}";
        }
    }
}

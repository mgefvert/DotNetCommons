namespace DotNetCommons.Collections;

/// A caching mechanism that allows for storing and retrieving collections of objects by type and key.
public class ObjectCache
{
    private sealed record CacheKey(Type Type, object Key);

    private sealed class CacheBucket
    {
        internal readonly DateTime Created = DateTime.UtcNow;
        internal readonly object Item;

        internal CacheBucket(object item)
        {
            Item = item;
        }
    }

    private readonly ReaderWriterLockSlim _lockObject = new();
    private readonly Dictionary<CacheKey, CacheBucket> _cache = new();
    private readonly TimeSpan _checkTimeout;
    private readonly TimeSpan _expireTimeout;
    private DateTime _nextPurge = DateTime.UtcNow;

    /// Create a new ObjectCache with sensible defaults for expiring objects (1 hour).
    public ObjectCache() : this(TimeSpan.FromHours(1), TimeSpan.FromMinutes(1))
    {
    }

    /// <summary>
    /// Create a new ObjectCache with specific expiration timeouts.
    /// </summary>
    /// <param name="expireTimeout">The timeout for when objects automatically expire.</param>
    /// <param name="checkTimeout">How often we should check for object expiration. Should be substantially less
    /// than the expireTimeout parameter.</param>
    public ObjectCache(TimeSpan expireTimeout, TimeSpan checkTimeout)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expireTimeout, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(checkTimeout, TimeSpan.Zero);

        _checkTimeout  = checkTimeout;
        _expireTimeout = expireTimeout;
    }

    private void InternalExpireBuckets()
    {
        var now = DateTime.UtcNow;
        if (now < _nextPurge)
            return;

        _lockObject.EnterWriteLock();
        try
        {
            if (now < _nextPurge)
                return;

            _nextPurge = now + _checkTimeout;
            var keys = _cache
                .Where(item => now - item.Value.Created >= _expireTimeout)
                .Select(item => item.Key)
                .ToArray();

            foreach (var key in keys)
                _cache.Remove(key);
        }
        finally
        {
            _lockObject.ExitWriteLock();
        }
    }

    private void InternalSetBucket(CacheKey key, object item)
    {
        _lockObject.EnterWriteLock();
        try
        {
            _cache[key] = new CacheBucket(item);
        }
        finally
        {
            _lockObject.ExitWriteLock();
        }
    }

    /// Caches a single item under a default, empty key as an array.
    /// If items already exist under the default key, they will be overridden.
    /// This is a shorthand for adding a single item, which converts it into an array internally.
    public void Cache<T>(T item)
    {
        Cache("", [item]);
    }

    /// Caches an array of items under a default, empty key.
    /// If items already exist under the default key, they will be overridden.
    public void Cache<T>(T[] items)
    {
        Cache("", items);
    }

    /// Caches a single item under a specific key as an array.
    /// If items already exist under the given key, they will be overridden.
    /// This is a shorthand for adding a single item, which converts it into an array internally.
    public void Cache<T>(string key, T item)
    {
        Cache(key, [item]);
    }

    /// Caches an array of items under a specific key.
    /// If items already exist under the given key, they will be overridden.
    public void Cache<T>(string key, T[] items)
    {
        _lockObject.EnterUpgradeableReadLock();
        try
        {
            InternalExpireBuckets();
            InternalSetBucket(new CacheKey(typeof(T), key), items);
        }
        finally
        {
            _lockObject.ExitUpgradeableReadLock();
        }
    }

    /// Retrieve all cached objects of the specified type with the default, empty key.
    /// If no objects of the given type have been cached, returns null.
    public T[]? Get<T>()
    {
        return Get<T>("");
    }

    /// Retrieve all cached objects of the specified type with a specific key.
    /// If no objects of the given type have been cached, returns null.
    public T[]? Get<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        _lockObject.EnterUpgradeableReadLock();
        try
        {
            InternalExpireBuckets();

            return _cache.TryGetValue(new CacheKey(typeof(T), key), out var bucket)
                ? (T[])bucket.Item
                : null;
        }
        finally
        {
            _lockObject.ExitUpgradeableReadLock();
        }
    }

    /// Immediately evict all cached objects.
    public void InvalidateAll()
    {
        _lockObject.EnterWriteLock();
        try
        {
            _cache.Clear();
        }
        finally
        {
            _lockObject.ExitWriteLock();
        }
    }

    /// Immediately evict all cached objects of a specific type, regardless of key.
    public void InvalidateAllOfType<T>()
    {
        _lockObject.EnterWriteLock();
        try
        {
            var keys = _cache.Where(x => x.Key.Type == typeof(T)).Select(x => x.Key).ToList();
            foreach (var key in keys)
                _cache.Remove(key);
        }
        finally
        {
            _lockObject.ExitWriteLock();
        }
    }

    /// Immediately evict all cached objects of a specific type, with the default, empty key.
    public void Invalidate<T>()
    {
        Invalidate<T>("");
    }

    /// Immediately evict all cached objects of a specific type and a specific key.
    public void Invalidate<T>(string key)
    {
        _lockObject.EnterWriteLock();
        try
        {
            _cache.Remove(new CacheKey(typeof(T), key));
        }
        finally
        {
            _lockObject.ExitWriteLock();
        }
    }
}
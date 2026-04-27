namespace DotNetCommons.Collections;

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

    public ObjectCache() : this(TimeSpan.FromHours(1), TimeSpan.FromMinutes(1))
    {
    }

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

    public void Cache<T>(T item)
    {
        Cache("", [item]);
    }

    public void Cache<T>(T[] items)
    {
        Cache("", items);
    }

    public void Cache<T>(string key, T item)
    {
        Cache(key, [item]);
    }

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

    public T[]? Get<T>()
    {
        return Get<T>("");
    }

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

    public void Invalidate<T>()
    {
        Invalidate<T>("");
    }

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
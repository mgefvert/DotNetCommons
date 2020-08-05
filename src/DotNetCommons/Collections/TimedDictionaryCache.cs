using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ODataService.Classes
{
    /// <summary>
    /// This class implements a dictionary cache that has a timeout capability. Any entry that is older than
    /// a given time will be automatically purged from the dictionary. It is also heavily geared for concurrent
    /// async operations. It supports a load callback that can be used to load objects from an asynchronous
    /// source - all tasks requesting this key will pause, while waiting for the load. This makes this class
    /// ideal for maintaining a cache of expensive objects with a limited life span.
    /// </summary>
    /// <typeparam name="TKey">Key to use.</typeparam>
    /// <typeparam name="TValue">Object to cache.</typeparam>
    public class TimedDictionaryCache<TKey, TValue> : IDictionaryCache<TKey, TValue> where TValue : class
    {
        private class ValueWrapper
        {
            public readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);
            public DateTime Timestamp = DateTime.UtcNow;
            public TValue Value;
        }

        private DateTime _lastPurge;
        private readonly TimeSpan _purgeAfter;
        private readonly TimeSpan _purgeCheck;
        private readonly ConcurrentDictionary<TKey, ValueWrapper> _items;

        public Func<TKey, Task<TValue>> LoadObject { get; set; }

        /// <summary>
        /// Initialize the TimedDictionaryCache.
        /// </summary>
        /// <param name="purgeAfter">A TimeSpan after which entries will be automatically purged from the dictionary.</param>
        /// <param name="comparer">Optional comparer for keys.</param>
        public TimedDictionaryCache(TimeSpan purgeAfter, IEqualityComparer<TKey> comparer = null)
        {
            _items = new ConcurrentDictionary<TKey, ValueWrapper>(comparer);
            _purgeAfter = purgeAfter;
            _purgeCheck = TimeSpan.FromTicks(_purgeAfter.Ticks / 10);
        }

        /// <summary>
        /// Return the count of all the objects in the dictionary.
        /// </summary>
        public int Count()
        {
            Purge();
            return _items.Count;
        }

        /// <summary>
        /// Immediately clear all objects from the list. Does not affect objects that
        /// are currently in the process of being loaded using the load callback.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _lastPurge = DateTime.UtcNow;
        }

        /// <summary>
        /// Check if a key exists in the dictionary.
        /// </summary>
        public bool Exists(TKey key)
        {
            Purge();
            return _items.ContainsKey(key);
        }

        /// <summary>
        /// Fetch an object from the dictionary; or null if it doesn't exist. Will attempt to load
        /// object asynchronously if a loader callback was provided.
        /// </summary>
        public async Task<TValue> Get(TKey key)
        {
            Purge();

            ValueWrapper wrapper;

            // If we can't load objects, just do a simple cache hit/miss.
            if (LoadObject == null)
            {
                if (!_items.TryGetValue(key, out wrapper))
                    return null;

                await wrapper.Lock.WaitAsync();
                try
                {
                    return wrapper.Value;
                }
                finally
                {
                    wrapper.Lock.Release();
                }
            }

            // We can load values. Load a wrapper or create if it doesn't exist.
            wrapper = _items.GetOrAdd(key, k => new ValueWrapper());

            await wrapper.Lock.WaitAsync();
            try
            {
                if (wrapper.Value != null)
                    return wrapper.Value;

                wrapper.Value = await LoadObject(key);
                return wrapper.Value;
            }
            finally
            {
                wrapper.Lock.Release();
            }
        }

        /// <summary>
        /// Purge all stale entries from the dictionary. Usually not necessary as this method is automatically
        /// invoked whenever any method is called. Has an internal check to ensure that it's not needlessly running
        /// an expensive check on all keys (1/10th of the purgeAfter value).
        /// </summary>
        public void Purge()
        {
            if (DateTime.UtcNow - _lastPurge < _purgeCheck)
                return;

            var now = DateTime.UtcNow;
            var removeKeys = _items
                .Where(x => now - x.Value.Timestamp > _purgeAfter)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in removeKeys)
                _items.TryRemove(key, out _);
        }

        /// <summary>
        /// Set a given key to a specific object.
        /// </summary>
        public async Task Set(TKey key, TValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Purge();

            var wrapper = _items.GetOrAdd(key, k => new ValueWrapper());

            await wrapper.Lock.WaitAsync();
            try
            {
                wrapper.Timestamp = DateTime.UtcNow;
                wrapper.Value = value;
            }
            finally
            {
                wrapper.Lock.Release();
            }
        }
    }
}

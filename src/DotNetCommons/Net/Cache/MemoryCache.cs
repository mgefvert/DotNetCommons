using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DotNetCommons.Net.Cache
{
    public class MemoryCache : IWebCache
    {
        protected readonly Dictionary<string, CacheItem> _store = new Dictionary<string, CacheItem>();
        protected readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public bool Changed { get; protected set; }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _store.Clear();
                Changed = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clean(TimeSpan age)
        {
            _lock.EnterWriteLock();
            try
            {
                var now = DateTime.UtcNow;
                foreach (var item in _store.ToList())
                {
                    if (now - item.Value.Timestamp > age)
                        Changed = Changed || _store.Remove(item.Key);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Exists(string uri)
        {
            _lock.EnterReadLock();
            try
            {
                return _store.ContainsKey(uri);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public CommonWebResult Fetch(string uri)
        {
            _lock.EnterReadLock();
            try
            {
                return _store.TryGetValue(uri, out var result) ? result.Result : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Remove(string uri)
        {
            _lock.EnterWriteLock();
            try
            {
                var result = _store.Remove(uri);
                if (result)
                    Changed = true;
                return result;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Store(string uri, CommonWebResult result)
        {
            _lock.EnterWriteLock();
            try
            {
                _store[uri] = new CacheItem
                {
                    Uri = uri,
                    Timestamp = DateTime.UtcNow,
                    Result = result
                };
                Changed = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryFetch(string uri, out CommonWebResult result)
        {
            _lock.EnterReadLock();
            try
            {
                if (_store.TryGetValue(uri, out var item))
                {
                    result = item.Result;
                    return true;
                }

                result = null;
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}

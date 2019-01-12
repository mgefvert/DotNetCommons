using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net.Cache
{
    public class MemoryCache : IWebCache
    {
        protected readonly Dictionary<string, CacheItem> InternalStore = new Dictionary<string, CacheItem>();
        protected readonly ReaderWriterLockSlim InternalLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public bool Changed { get; protected set; }

        public void Clear()
        {
            InternalLock.EnterWriteLock();
            try
            {
                InternalStore.Clear();
                Changed = true;
            }
            finally
            {
                InternalLock.ExitWriteLock();
            }
        }

        public void Clean(TimeSpan age)
        {
            InternalLock.EnterWriteLock();
            try
            {
                var now = DateTime.UtcNow;
                foreach (var item in InternalStore.ToList())
                {
                    if (now - item.Value.Timestamp > age)
                        Changed = Changed || InternalStore.Remove(item.Key);
                }
            }
            finally
            {
                InternalLock.ExitWriteLock();
            }
        }

        public bool Exists(string uri)
        {
            InternalLock.EnterReadLock();
            try
            {
                return InternalStore.ContainsKey(uri);
            }
            finally
            {
                InternalLock.ExitReadLock();
            }
        }

        public CommonWebResult Fetch(string uri)
        {
            InternalLock.EnterReadLock();
            try
            {
                return InternalStore.TryGetValue(uri, out var result) ? result.Result : null;
            }
            finally
            {
                InternalLock.ExitReadLock();
            }
        }

        public bool Remove(string uri)
        {
            InternalLock.EnterWriteLock();
            try
            {
                var result = InternalStore.Remove(uri);
                if (result)
                    Changed = true;
                return result;
            }
            finally
            {
                InternalLock.ExitWriteLock();
            }
        }

        public void Store(string uri, CommonWebResult result)
        {
            InternalLock.EnterWriteLock();
            try
            {
                InternalStore[uri] = new CacheItem
                {
                    Uri = uri,
                    Timestamp = DateTime.UtcNow,
                    Result = result
                };
                Changed = true;
            }
            finally
            {
                InternalLock.ExitWriteLock();
            }
        }

        public bool TryFetch(string uri, out CommonWebResult result)
        {
            InternalLock.EnterReadLock();
            try
            {
                if (InternalStore.TryGetValue(uri, out var item))
                {
                    result = item.Result;
                    return true;
                }

                result = null;
                return false;
            }
            finally
            {
                InternalLock.ExitReadLock();
            }
        }
    }
}

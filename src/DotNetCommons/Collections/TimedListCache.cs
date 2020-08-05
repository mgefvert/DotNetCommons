//#define DEBUG_TIMEDLISTCACHE

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ODataService.Classes
{
    public class TimedListCache<T> : IListCache<T>
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private List<T> _items;
        private readonly TimeSpan _purgeAfter;
        private DateTime _purgeNext;
        
        public Func<Task<List<T>>> LoadObject { get; set; }

        public TimedListCache(TimeSpan purgeAfter)
        {
            _purgeAfter = purgeAfter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Debug(string msg)
        {
#if DEBUG_TIMEDLISTCACHE
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{Task.CurrentId}] {msg}");
#endif
        }

        public async Task<int> Count()
        {
            Debug("Count");
            await Purge();

            await _lock.WaitAsync();
            try
            {
                return _items?.Count ?? 0;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task Clear()
        {
            Debug("Clear");
            
            await _lock.WaitAsync();
            try
            {
                _items = null;
                _purgeNext = DateTime.MinValue;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> Exists()
        {
            Debug("Exists");
            await Purge();

            await _lock.WaitAsync();
            try
            {
                return _items != null;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<List<T>> Get()
        {
            Debug("Get");
            await Purge();
            return await InternalGet();
        }

        private async Task<List<T>> InternalGet()
        {
            if (_items == null && LoadObject == null)
                return null;

            await _lock.WaitAsync();
            try
            {
                if (_items == null && LoadObject != null)
                {
                    // Load a new list
                    Debug("InternalGet.LoadObject");
                    _items = await LoadObject();
                    _purgeNext = DateTime.UtcNow.Add(_purgeAfter);

                    Debug("InternalGet.Return new => " + (_items == null ? "(null)" : string.Join(",", _items)));
                    return _items;
                }

                Debug("InternalGet.Return => " + (_items == null ? "(null)" : string.Join(",", _items)));
                return _items;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task Purge()
        {
            if (_items == null || DateTime.UtcNow < _purgeNext)
                return;

            await _lock.WaitAsync();
            try
            {
                if (_items == null || DateTime.UtcNow < _purgeNext)
                    return;

                Debug("Purging");
                _items = null;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task Set(List<T> value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Debug("Set");
            await _lock.WaitAsync();
            try
            {
                _items = value;
                _purgeNext = DateTime.UtcNow.Add(_purgeAfter);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}

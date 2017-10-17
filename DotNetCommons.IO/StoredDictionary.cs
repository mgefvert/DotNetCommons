using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetCommons.IO
{
    public class StoredDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
    {
        private readonly KeyValueStore _store = new KeyValueStore();
        public string Filename { get; set; }

        public StoredDictionary(string filename) : this(filename, null)
        {
        }

        public StoredDictionary(string filename, IEqualityComparer<TKey> comparer) : base(comparer)
        {
            Filename = filename;

            if (File.Exists(Filename))
                foreach (var item in _store.Load<TKey, TValue>(Filename))
                    this[item.Key] = item.Value;
        }

        public void Dispose()
        {
            _store.Save(this, Filename);
        }
    }
}

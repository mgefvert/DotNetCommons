using System;
using System.Collections;
using System.Collections.Generic;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Collections
{
    public class IndexedListItem<T>
    {
        private readonly Func<T, object> _accessor;
        private readonly Dictionary<object, List<T>> _index = new Dictionary<object, List<T>>();

        internal IndexedListItem(Func<T, object> accessor)
        {
            _accessor = accessor;
        }

        private List<T> Access(T item, bool create)
        {
            var key = _accessor(item);

            if (!_index.TryGetValue(key, out var index) && create)
            {
                index = new List<T>();
                _index[key] = index;
            }

            return index;
        }

        internal void Add(T item)
        {
            Access(item, true).Add(item);
        }

        internal void Clear()
        {
            _index.Clear();
        }

        internal List<T> Lookup(object value)
        {
            return _index.TryGetValue(value, out var result)
              ? result
              : new List<T>();
        }

        internal void Remove(T item)
        {
            Access(item, false)?.Remove(item);
        }

        public List<T> this[object key] => Lookup(key);
    }

    public class IndexedList<T> : IList<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly Dictionary<string, IndexedListItem<T>> _indexes = new Dictionary<string, IndexedListItem<T>>();

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public void DefineIndex(string indexName, Func<T, object> accessor)
        {
            var index = new IndexedListItem<T>(accessor);
            foreach (var item in _list)
                index.Add(item);

            _indexes[indexName] = index;
        }

        public IndexedListItem<T> FindIndex(string indexName)
        {
            return _indexes[indexName];
        }

        public List<T> Lookup(string indexName, object key)
        {
            if (!_indexes.TryGetValue(indexName, out var index))
                throw new ArgumentException("Index '" + indexName + "' is not defined", nameof(indexName));

            return index.Lookup(key);
        }

        private void AddToIndex(T item)
        {
            foreach (var index in _indexes.Values)
                index.Add(item);
        }

        private void RemoveFromIndex(T item)
        {
            foreach (var index in _indexes.Values)
                index.Remove(item);
        }

        public void Add(T item)
        {
            _list.Add(item);
            AddToIndex(item);
        }

        public void Clear()
        {
            _list.Clear();
            foreach (var index in _indexes.Values)
                index.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int position, T item)
        {
            _list.Insert(position, item);
            AddToIndex(item);
        }

        public bool Remove(T item)
        {
            var result = _list.Remove(item);
            if (result)
                RemoveFromIndex(item);

            return result;
        }

        public void RemoveAt(int index)
        {
            Remove(_list[index]);
        }

        public T this[int index]
        {
            get => _list[index];
            set
            {
                RemoveFromIndex(_list[index]);
                _list[index] = value;
                AddToIndex(value);
            }
        }
    }
}

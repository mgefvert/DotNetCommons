using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections;

/// <summary>
/// Implements a list of data with fast lookup indexes.
/// </summary>
/// <typeparam name="T"></typeparam>
public class IndexedCollection<T> : IEnumerable<T>
{
    private class InternalIndex
    {
        private readonly Func<T, object> _accessor;
        private readonly Dictionary<object, List<T>> _index = new();

        internal InternalIndex(Func<T, object> accessor)
        {
            _accessor = accessor;
        }

        private List<T> Access(T item, bool createKey)
        {
            var key = _accessor(item);

            if (!_index.TryGetValue(key, out var index) && createKey)
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
                ? result.ToList()
                : new List<T>();
        }

        internal void Remove(T item)
        {
            Access(item, false)?.Remove(item);
        }
    }

    private readonly HashSet<T> _list = new();
    private readonly Dictionary<string, InternalIndex> _indexes = new();

    public int Count => _list.Count;
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    public bool IsReadOnly => false;

    /// <summary>
    /// Add a certain object to the list.
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        _list.Add(item);
        foreach (var index in _indexes.Values)
            index.Add(item);
    }

    /// <summary>
    /// Add a certain object to the list.
    /// </summary>
    /// <param name="items"></param>
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            Add(item);
    }

    /// <summary>
    /// Clear the list of all objects.
    /// </summary>
    public void Clear()
    {
        _list.Clear();
        foreach (var index in _indexes.Values)
            index.Clear();
    }

    /// <summary>
    /// Check to see if the list contains a certain object.
    /// </summary>
    /// <param name="item">Object to look for.</param>
    /// <returns>True or false.</returns>
    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    /// <summary>
    /// Define an index, by registering an index name and an access method that can pull the value
    /// out of a data object.
    /// </summary>
    /// <param name="indexName">Name of index</param>
    /// <param name="accessor">Function that can extract a single value out of a data object.</param>
    public void DefineIndex(string indexName, Func<T, object> accessor)
    {
        var internalIndex = new InternalIndex(accessor);
        foreach (var item in _list)
            internalIndex.Add(item);

        _indexes.Add(indexName, internalIndex);
    }

    /// <summary>
    /// Look up a value for a specific index, and return the objects found.
    /// </summary>
    /// <param name="index">The index to use.</param>
    /// <param name="key">The value to look for.</param>
    /// <returns>All the objects found.</returns>
    public List<T> Lookup(string index, object key)
    {
        if (!_indexes.TryGetValue(index, out var internalIndex))
            throw new ArgumentException($"Index {index} is not defined", nameof(index));

        return internalIndex.Lookup(key);
    }

    /// <summary>
    /// Remove a certain object from the list.
    /// </summary>
    /// <param name="item">Object to remove.</param>
    /// <returns>Whether the object was found or not.</returns>
    public bool Remove(T item)
    {
        var result = _list.Remove(item);
        if (result)
        {
            foreach (var index in _indexes.Values)
                index.Remove(item);
        }

        return result;
    }

    /// <summary>
    /// Remove all objects with a given key.
    /// </summary>
    /// <param name="index">Index to use.</param>
    /// <param name="key">Key to look for.</param>
    /// <returns>Number of objects removed.</returns>
    public int Remove(string index, object key)
    {
        var items = Lookup(index, key);
        foreach (var item in items)
            Remove(item);

        return items.Count;
    }

    /// <summary>
    /// Remove an index from the list.
    /// </summary>
    /// <param name="index">Name of index,</param>
    public void RemoveIndex(string index)
    {
        _indexes.Remove(index);
    }
}
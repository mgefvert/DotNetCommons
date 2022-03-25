﻿using System;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections;

public class DrawList<T>
{
    private readonly object _lock = new();
    private readonly Random _rnd = new();
    private readonly List<T> _source = new();
    private readonly List<T> _current = new();

    public bool Repeat { get; set; } = true;

    public DrawList()
    {
    }

    public DrawList(IEnumerable<T> source)
    {
        Seed(source);
    }

    public int Count()
    {
        lock (_lock)
        {
            return _source.Count;
        }
    }

    public T? Draw()
    {
        lock (_lock)
        {
            if (_source.Count == 0 || (!Repeat && _current.Count == 0))
                return default;

            var n = _rnd.Next(_current.Count);
            var s = _current[n];
            _current.RemoveAt(n);

            if (Repeat && _current.Count == 0)
                _current.AddRange(_source);

            return s;
        }
    }

    public int Left()
    {
        lock (_lock)
        {
            return _current.Count;
        }
    }

    public void Seed(IEnumerable<T> items)
    {
        lock (_lock)
        {
            _source.Clear();
            _source.AddRange(items);
            _current.Clear();
            _current.AddRange(_source);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections;

public class CircularBufferFullException : Exception
{
    public CircularBufferFullException() : base("Circular buffer is full")
    {
    }
}

public class CircularBufferEmptyException : Exception
{
    public CircularBufferEmptyException() : base("Circular buffer is empty")
    {
    }
}

public class CircularBuffer<T>
{
    private readonly T[] _items;
    private int _readPosition;
    private int _writePosition;

    public bool Empty => !Full && _readPosition == _writePosition;

    public bool Full { get; private set; }

    public int Size => _items.Length;

    public CircularBuffer(int size)
    {
        if (size < 1)
            throw new ArgumentOutOfRangeException(nameof(size));

        _items = new T[size];
    }

    private void Advance(ref int position)
    {
        position++;
        if (position >= Size)
            position = 0;
    }

    private void AssertNotEmpty()
    {
        if (Empty)
            throw new CircularBufferEmptyException();
    }

    private void AssertNotFull()
    {
        if (Full)
            throw new CircularBufferFullException();
    }

    public void Clear()
    {
        _writePosition = 0;
        _readPosition = 0;
        Full = false;
    }

    public bool Contains(T result)
    {
        return Values().Any(v => v != null && v.Equals(result));
    }

    public int Count
    {
        get
        {
            if (Full)
                return Size;

            var result = _writePosition - _readPosition;
            if (result < 0)
                result += Size;

            return result;
        }
    }

    public T First
    {
        get
        {
            AssertNotEmpty();
            return _items[_readPosition];
        }
    }

    public T Last
    {
        get
        {
            AssertNotEmpty();
            return _items[Prev(_writePosition)];
        }
    }

    private int Prev(int position)
    {
        return position > 0 ? position - 1 : Size - 1;
    }

    public T Read()
    {
        AssertNotEmpty();

        var result = _items[_readPosition];

        Advance(ref _readPosition);
        Full = false;

        return result;
    }

    public IEnumerable<T> Values()
    {
        if (Empty)
            yield break;

        var current = _readPosition;
        do
        {
            yield return _items[current];
            Advance(ref current);
        } while (current != _writePosition);
    }

    public void Write(T value)
    {
        AssertNotFull();

        _items[_writePosition] = value;

        Advance(ref _writePosition);
        Full = _writePosition == _readPosition;
    }
}
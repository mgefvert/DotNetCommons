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

/// <summary>
/// A fixed size circular buffer (reminiscent of a UART-16550 input buffer).
/// </summary>
public class CircularBuffer<T>
{
    private readonly T[] _items;
    private int _readPosition;
    private int _writePosition;

    /// <summary>
    /// Any items available in the buffer?
    /// </summary>
    public bool Empty => !Full && _readPosition == _writePosition;

    /// <summary>
    /// Is the buffer full?
    /// </summary>
    public bool Full { get; private set; }

    /// <summary>
    /// Current size of the circular buffer (the total number of elements that can be stored in it).
    /// </summary>
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

    /// <summary>
    /// Clear the buffer.
    /// </summary>
    public void Clear()
    {
        _writePosition = 0;
        _readPosition = 0;
        Full = false;
    }

    /// <summary>
    /// Check to see whether the buffer contains an item.
    /// </summary>
    public bool Contains(T result)
    {
        return Values().Any(v => v != null && v.Equals(result));
    }

    /// <summary>
    /// The total number of items currently in the buffer.
    /// </summary>
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

    /// <summary>
    /// First item read non-destructively.
    /// </summary>
    public T First
    {
        get
        {
            AssertNotEmpty();
            return _items[_readPosition];
        }
    }

    /// <summary>
    /// Last item read non-destructively.
    /// </summary>
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

    /// <summary>
    /// Read a single item from the buffer, or throw a <see cref="CircularBufferEmptyException"/> if
    /// the buffer is empty.
    /// </summary>
    public T Read()
    {
        AssertNotEmpty();

        var result = _items[_readPosition];

        Advance(ref _readPosition);
        Full = false;

        return result;
    }

    /// <summary>
    /// Enumerate the values in the buffer non-destructively.
    /// </summary>
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

    /// <summary>
    /// Write an item to the buffer, or throw a <see cref="CircularBufferFullException"/> if
    /// the buffer is full.
    /// </summary>
    public void Write(T value)
    {
        AssertNotFull();

        _items[_writePosition] = value;

        Advance(ref _writePosition);
        Full = _writePosition == _readPosition;
    }
}
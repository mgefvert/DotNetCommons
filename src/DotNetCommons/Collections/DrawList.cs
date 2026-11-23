// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections;

/// Represents a list of objects, where the individual items are drawn in a random order.
/// Repeats if so desired. Thread-safe.
public class DrawList<T>
{
    private readonly object _lock = new();
    private readonly Random _rnd = new();
    private readonly List<T> _source = [];
    private readonly List<T> _current = [];

    public bool Repeat { get; set; } = true;

    public DrawList()
    {
    }

    public DrawList(IEnumerable<T> source)
    {
        Seed(source);
    }

    public void Clear()
    {
        Seed([]);
    }

    /// The number of items available in the source list.
    public int Count()
    {
        lock (_lock)
        {
            return _source.Count;
        }
    }

    private T? InternalDraw(int index)
    {
        if (_source.Count == 0 || (!Repeat && _current.Count == 0) || index >= _current.Count || index < 0)
            return default;

        var s = _current[index];
        _current.RemoveAt(index);

        if (Repeat && _current.Count == 0)
            _current.AddRange(_source);

        return s;
    }

    /// Draw a new item randomly from the list. If repeating, there will
    /// always be a new item drawn at random; if non-repeating, a null or default
    /// value will be returned if the list is empty.
    public T? Draw()
    {
        lock (_lock)
        {
            return InternalDraw(_rnd.Next(_current.Count));
        }
    }

    /// Draw a new item randomly from the list. If repeating, there will
    /// always be a new item drawn at random; if non-repeating, a null or default
    /// value will be returned if the list is empty.
    public List<T> Draw(int count)
    {
        var result = new List<T>();
        lock (_lock)
        {
            for (var i = 0; i < count; i++)
                result.AddIfNotNull(InternalDraw(_rnd.Next(_current.Count)));
        }

        return result;
    }

    /// Draw a specific item at position index and remove it from the list.
    public T? DrawIndex(int index)
    {
        lock (_lock)
        {
            return InternalDraw(index);
        }
    }

    /// Number of items left in the draw buffer.
    public int Left()
    {
        lock (_lock)
        {
            return _current.Count;
        }
    }

    /// Seed the draw buffer with new records (will erase the current items and state).
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
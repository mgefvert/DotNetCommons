// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections;

/// <summary>
/// Represents a list of objects, where the individual items are drawn in a random order.
/// Repeats if so desired. Thread-safe.
/// </summary>
/// <typeparam name="T"></typeparam>
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

    /// <summary>
    /// The number of items available in the source list.
    /// </summary>
    public int Count()
    {
        lock (_lock)
        {
            return _source.Count;
        }
    }

    /// <summary>
    /// Draw a new item randomly from the list. If repeating, there will
    /// always be a new item drawn at random; if non-repeating, a null or default
    /// value will be returned if the list is empty.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Number of items left in the draw buffer.
    /// </summary>
    public int Left()
    {
        lock (_lock)
        {
            return _current.Count;
        }
    }

    /// <summary>
    /// Seed the draw buffer with new records (will erase the current items and state).
    /// </summary>
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
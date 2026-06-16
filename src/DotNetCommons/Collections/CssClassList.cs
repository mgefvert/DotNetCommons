namespace DotNetCommons.Collections;

public class CssClassList
{
    private readonly HashSet<string> _classes = [];

    public IEnumerable<string> Items => _classes.Order();

    public string Text
    {
        get => string.Join(" ", _classes.Order());
        set => Set(value);
    }

    public CssClassList()
    {
    }

    public CssClassList(string classes)
    {
        Add(classes);
    }

    public CssClassList(IEnumerable<string> classes)
    {
        Add(classes);
    }

    private string[] MakeList(string? classes) => classes.IsEmpty() ? [] : classes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();

    private string[] MakeList(IEnumerable<string>? classes) => classes?.SelectMany(MakeList).Distinct().ToArray() ?? [];

    public CssClassList Add(string? classes)
    {
        foreach (var c in MakeList(classes))
            _classes.Add(c);
        return this;
    }

    public CssClassList Add(IEnumerable<string> classes)
    {
        foreach (var c in MakeList(classes))
            _classes.Add(c);
        return this;
    }

    public CssClassList Clear()
    {
        _classes.Clear();
        return this;
    }

    public bool Contains(string? classes)
    {
        return !classes.IsEmpty() && _classes.IsSupersetOf(MakeList(classes));
    }

    public bool Contains(IEnumerable<string> classes)
    {
        var list = MakeList(classes);
        return list.IsAtLeastOne() && _classes.IsSupersetOf(list);
    }

    public CssClassList Remove(string? classes)
    {
        foreach (var c in MakeList(classes))
            _classes.Remove(c);
        return this;
    }

    public CssClassList Remove(IEnumerable<string> classes)
    {
        foreach (var c in MakeList(classes))
            _classes.Remove(c);
        return this;
    }

    public CssClassList Set(string? classes)
    {
        Clear();
        Add(classes);
        return this;
    }

    public CssClassList Set(IEnumerable<string> classes)
    {
        Clear();
        Add(classes);
        return this;
    }

    public CssClassList Toggle(string? classes)
    {
        var list = MakeList(classes);
        var (removes, adds) = list.Toss(_classes.Contains);

        foreach (var c in adds)
            _classes.Add(c);
        foreach (var c in removes)
            _classes.Remove(c);

        return this;
    }

    public CssClassList Toggle(IEnumerable<string> classes)
    {
        var list = MakeList(classes);
        var (removes, adds) = list.Toss(_classes.Contains);

        foreach (var c in adds)
            _classes.Add(c);
        foreach (var c in removes)
            _classes.Remove(c);

        return this;
    }

    public override string ToString() => Text;
}
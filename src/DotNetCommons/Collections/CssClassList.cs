using System.Text.RegularExpressions;

namespace DotNetCommons.Collections;

public class CssClassList
{
    private readonly HashSet<string> _classes = [];

    /// Causes attributes with suffixes to replace each other; size-5 would replace size-3 classes,
    /// w-full would replace w-1/2 and so on.
    public bool TailwindLogic { get; set; }

    private static readonly Regex TailwindClassPattern = new(@"^(?<prefix>.+)-(?<suffix>full|auto|percent|\d+%?|\d+/\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IEnumerable<string> Items => _classes.Order();

    public string Text
    {
        get => string.Join(" ", _classes.Order());
        set => Set(value);
    }

    public static string Combine(params string?[] classes) => Combine(false, classes);

    public static string Combine(bool tailwind, params string?[] classes)
    {
        var result = new CssClassList { TailwindLogic = tailwind };
        result.Add(classes.NotNulls());
        return result.Text;
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
            AddClass(c);
        return this;
    }

    public CssClassList Add(IEnumerable<string> classes)
    {
        foreach (var c in MakeList(classes))
            AddClass(c);
        return this;
    }

    private void AddClass(string @class)
    {
        if (TailwindLogic && GetTailwindClassPrefix(@class) is { } prefix)
            _classes.RemoveWhere(existing => GetTailwindClassPrefix(existing) == prefix);

        _classes.Add(@class);
    }

    private static string? GetTailwindClassPrefix(string @class)
    {
        var match = TailwindClassPattern.Match(@class);
        return match.Success ? match.Groups["prefix"].Value : null;
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
            AddClass(c);
        foreach (var c in removes)
            _classes.Remove(c);

        return this;
    }

    public CssClassList Toggle(IEnumerable<string> classes)
    {
        var list = MakeList(classes);
        var (removes, adds) = list.Toss(_classes.Contains);

        foreach (var c in adds)
            AddClass(c);
        foreach (var c in removes)
            _classes.Remove(c);

        return this;
    }

    public override string ToString() => Text;
}

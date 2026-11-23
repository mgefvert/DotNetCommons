namespace DotNetCommons.Base;

public class CssClassOverrides
{
    public string? Defaults { get; set { field  = value; UpdateList(); } }
    public string? Overrides { get; set { field = value; UpdateList(); } }
    public string? State { get; set { field = value; UpdateList(); } }

    private readonly CssClassList _list = new();
    private string _cachedString = "";

    private void UpdateList()
    {
        _list.Clear();
        if (!string.IsNullOrWhiteSpace(Defaults))
            _list.Add(Defaults);
        if (!string.IsNullOrWhiteSpace(Overrides))
            _list.Add(Overrides);
        if (!string.IsNullOrWhiteSpace(State))
            _list.Add(State);
        _cachedString = _list.ToString() ?? "";
    }

    public override string ToString() => _cachedString;
}

using System.Text.Json;
using DotNetCommons.Text;

namespace Fleet;

public class UnitStates : Dictionary<string, UnitState>
{
    private static readonly string[] Subs =
    [
        ..Enumerable.Range(1, 8).Select(i => $"ohio-{i}"),
        ..Enumerable.Range(1, 2).Select(i => $"columbia-{i}"),
        ..Enumerable.Range(1, 5).Select(i => $"los-angeles-{i}"),
    ];

    private static readonly string[] Wings =
    [
        "2nd-wing", "5th-wing", "307th-wing", "509th-wing"
    ];

    private void Initialize()
    {
        foreach (var name in Subs)
            this[name] = new UnitState(true);
        foreach (var name in Wings)
            this[name] = new UnitState(false);
    }

    /// <summary>
    /// Load UnitStates from a JSON file.
    /// </summary>
    public void Load(string fileName)
    {
        if (!File.Exists(fileName))
        {
            Initialize();
            return;
        }

        var json = File.ReadAllText(fileName);
        var dict = JsonSerializer.Deserialize<Dictionary<string, UnitState>>(json);
        if (dict == null)
            return;

        foreach (var (k, v) in dict)
            this[k] = v;
    }

    /// <summary>
    /// Save UnitStates to a JSON file.
    /// </summary>
    public void Save(string fileName)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fileName, json);
    }

    /// <summary>
    /// Selects units based on a provided name pattern or retrieves all units.
    /// </summary>
    /// <param name="unitName">The name pattern to match against unit names; wildcards like * and ? are accepted.</param>
    /// <param name="all">Indicates whether to select all units regardless of the pattern.</param>
    /// <returns>A list of unit names matching the specified pattern, or all unit names if <paramref name="all"/> is true.</returns>
    public List<KeyValuePair<string, UnitState>> SelectUnits(string? unitName, bool all)
    {
        if (all)
            return this.AsEnumerable().ToList();

        if (unitName == null)
            return [];

        var regex = Wildcards.ToRegex(unitName);
        return this
            .Where(item => regex.IsMatch(item.Key))
            .ToList();
    }
}
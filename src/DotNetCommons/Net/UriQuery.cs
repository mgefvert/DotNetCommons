using System.Collections;

namespace DotNetCommons.Net;

public class UriQuery : IEnumerable<KeyValuePair<string, string>>
{
    private readonly List<KeyValuePair<string, string?>> _keyValues;

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _keyValues.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public UriQuery()
    {
        _keyValues = [];
    }

    public UriQuery(string queryString) : this()
    {
        if (string.IsNullOrWhiteSpace(queryString))
            return;

        // Remove leading '?' if present
        if (queryString.StartsWith('?'))
            queryString = queryString.Substring(1);

        // Split by '&' to get individual parameters
        var pairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;

            Add(key, value);
        }
    }

    public UriQuery(IEnumerable<KeyValuePair<string, string?>> keyValues)
    {
        _keyValues = [.. keyValues];
    }

    public UriQuery(IDictionary<string, string?> keyValues)
    {
        _keyValues = [.. keyValues];
    }

    public UriQuery Add(string key, string? value)
    {
        _keyValues.Add(new KeyValuePair<string, string?>(key, value));
        return this;
    }

    public UriQuery Clear()
    {
        _keyValues.Clear();
        return this;
    }

    public string? Get(string key)
    {
        return _keyValues.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;
    }

    public IEnumerable<string?> GetAll(string key)
    {
        return _keyValues.Where(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
    }

    public UriQuery Remove(string key)
    {
        _keyValues.RemoveAll(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        return this;
    }

    public UriQuery Set(string key, string? value)
    {
        Remove(key);
        Add(key, value);
        return this;
    }

    public UriQuery Set<T>(string key, T value) where T : struct
    {
        Remove(key);
        Add(key, value.ToString());
        return this;
    }

    public UriQuery Set(string key, IEnumerable<string> values)
    {
        Remove(key);
        foreach (var v in values)
            Add(key, v);
        return this;
    }

    public UriQuery Set<T>(string key, IEnumerable<T> values) where T : struct
    {
        Remove(key);
        foreach (var v in values)
            Add(key, v.ToString());
        return this;
    }

    public override string ToString()
    {
        var strings = this
            .OrderBy(kvp => kvp.Key)
            .Where(kvp => kvp.Value != null)
            .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value!)}");

        return string.Join("&", strings);
    }
}
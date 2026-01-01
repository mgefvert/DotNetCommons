using System.Globalization;
using System.Reflection;
using System.Text;

namespace DotNetCommons.Text.FixedWidth;

public class FixedWidthConverter
{
    private class PropInfo(PropertyInfo prop, FixedWidthAttribute attr)
    {
        public PropertyInfo Prop { get; set; } = prop;
        public FixedWidthAttribute Attr { get; set; } = attr;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int Length { get; set; }
    }

    private static readonly Dictionary<Type, PropInfo[]> Definitions = [];

    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    private static PropInfo[] BuildDefinitions(Type type)
    {
        var fields = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => (Prop: p, Attr: p.GetCustomAttribute<FixedWidthAttribute>()))
            .Where(p => p.Attr != null)
            .Select(p => new PropInfo(p.Prop, p.Attr!))
            .OrderBy(p => p.Attr.Start)
            .ToArray();

        foreach (var f in fields)
        {
            f.StartIndex = f.Attr.Start - 1;
            f.Length = f.Attr.Length;
            f.EndIndex = f.StartIndex + f.Length;
        }

        // Check for overlaps
        for (var i=1; i<fields.Length; i++)
            if (fields[i].StartIndex < fields[i-1].EndIndex)
                throw new InvalidDataException(
                    $"Fixed width definitions for class {type.Name} overlap in position {fields[i].Attr!.Start}");

        return fields.Length == 0
            ? throw new InvalidDataException($"Class {type.Name} has no FixedWidth definitions")
            : fields;
    }

    public string Convert<T>(T item)
    {
        var info = GetPropInfo<T>();

        var maxlen = info.Last().EndIndex;
        var result = new StringBuilder(maxlen);
        for (int i = 0; i < maxlen; i++)
            result.Append(' ');

        foreach (var f in info)
        {
            var value = f.Prop.GetValue(item);
            var s = f.Attr.FormatValue(value, Culture);

            if (s.Length != f.Length)
                throw new InvalidDataException($"Invalid data length returned from formatter for value '{value}' => '{s}', " +
                    $"expected length {f.Length} but got {s.Length}.");

            for (var i = 0; i < s.Length; i++)
                result[f.StartIndex + i] = s[i];
        }

        return result.ToString();
    }

    private static PropInfo[] GetPropInfo<T>()
    {
        PropInfo[]? info;
        lock (Definitions)
        {
            if (!Definitions.TryGetValue(typeof(T), out info))
                Definitions[typeof(T)] = info = BuildDefinitions(typeof(T));
        }

        return info;
    }

    public T Parse<T>(string data)
        where T : class, new()
    {
        var info = GetPropInfo<T>();

        var maxlen = info.Last().EndIndex + 1;
        data = data.PadRight(maxlen);

        var result = new T();
        foreach (var f in info)
        {
            var s     = data.Substring(f.StartIndex, f.Length);
            var value = f.Attr.Parse(s, Culture);

            f.Prop.SetPropertyValue(result, value);
        }

        return result;
    }
}
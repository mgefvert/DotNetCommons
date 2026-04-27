using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
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

            if (value == null && f.Prop.PropertyType.IsNumeric() && !f.Prop.PropertyType.IsNullable())
                value = 0;

            try
            {
                f.Prop.SetPropertyValue(result, value);
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Data conversion failed for field {typeof(T).Name}.{f.Prop.Name}, data='{s}'.", e);
            }
        }

        return result;
    }

    public async IAsyncEnumerable<T> Read<T>(StreamReader reader, [EnumeratorCancellation] CancellationToken ct = default)
        where T : class, new()
    {
        while (!reader.EndOfStream)
        {
            var item = await ReadOne<T>(reader, ct);
            if (item != null)
                yield return item;
            else
                yield break;
        }
    }

    public async Task<T?> ReadOne<T>(StreamReader reader, CancellationToken ct = default)
        where T : class, new()
    {
        var line = await reader.ReadLineAsync(ct);
        return line == null ? null : Parse<T>(line);
    }

    public async Task Write<T>(StreamWriter writer, IEnumerable<T> items, CancellationToken ct = default)
    {
        foreach (var item in items)
        {
            if (ct.IsCancellationRequested)
                break;

            await WriteOne(writer, item);
        }
    }

    public async Task WriteOne<T>(StreamWriter writer, T item, CancellationToken ct = default)
    {
        var line = Convert(item);
        await writer.WriteLineAsync(line);
    }
}
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DotNetCommons.EF.DataReading;

public class TestDataReader
{
    private readonly record struct SectionBlock(string TypeName, bool IsDefault, List<string> Lines);

    private enum CellKind
    {
        Uninitialized,
        ExplicitNull,
        Text
    }

    private class TypeSection
    {
        public Dictionary<string, CellValue> Defaults { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<Dictionary<string, CellValue>> Rows { get; } = [];
    }

    private readonly record struct CellValue(CellKind Kind, string? Value)
    {
        public static CellValue Uninitialized => new(CellKind.Uninitialized, null);
        public static CellValue ExplicitNull => new(CellKind.ExplicitNull, null);
    }

    private readonly Dictionary<string, TypeSection> _sections = new(StringComparer.OrdinalIgnoreCase);

    private static bool CanBeNull(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

    public void Load(string filename)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        using var stream = File.OpenRead(filename);
        Load(stream);
    }

    public void Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new StreamReader(stream, leaveOpen: true);
        var content = reader.ReadToEnd();

        _sections.Clear();
        foreach (var block in ParseBlocks(content))
        {
            if (!_sections.TryGetValue(block.TypeName, out var section))
            {
                section = new TypeSection();
                _sections[block.TypeName] = section;
            }

            if (block.IsDefault)
                ApplyDefaults(section, block.Lines);
            else
                ApplyTable(section, block.Lines);
        }
    }

    public List<T> Fetch<T>() where T : class, new()
    {
        var type = typeof(T);
        if (!_sections.TryGetValue(type.Name, out var section))
            return [];

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => prop.CanWrite)
            .ToDictionary(prop => prop.Name, StringComparer.OrdinalIgnoreCase);

        var result = new List<T>(section.Rows.Count);
        foreach (var row in section.Rows)
        {
            var item = new T();

            foreach (var entry in section.Defaults)
                AssignValue(item, entry.Key, entry.Value, properties);

            foreach (var entry in row)
            {
                if (entry.Value.Kind == CellKind.Uninitialized)
                {
                    if (!section.Defaults.ContainsKey(entry.Key))
                        continue;

                    AssignValue(item, entry.Key, section.Defaults[entry.Key], properties);
                    continue;
                }

                AssignValue(item, entry.Key, entry.Value, properties);
            }

            result.Add(item);
        }

        return result;
    }

    private static void ApplyDefaults(TypeSection section, IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var idx = line.IndexOf('=');
            if (idx <= 0)
                continue;

            var key = line[..idx].Trim();
            var raw = line[(idx + 1)..].Trim();
            if (key.Length == 0)
                continue;

            section.Defaults[key] = ParseCell(raw);
        }
    }

    private static void ApplyTable(TypeSection section, IReadOnlyList<string> lines)
    {
        var rows = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        if (rows.Count == 0)
            return;

        var headers = ParseMarkdownRow(rows[0]);
        if (headers.Count == 0)
            return;

        var startRow = 1;
        if (rows.Count > 1 && IsSeparatorRow(ParseMarkdownRow(rows[1])))
            startRow = 2;

        for (var i = startRow; i < rows.Count; i++)
        {
            var values = ParseMarkdownRow(rows[i]);
            if (values.Count == 0 || IsSeparatorRow(values))
                continue;

            var row = new Dictionary<string, CellValue>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Count; c++)
            {
                var header = headers[c];
                if (string.IsNullOrWhiteSpace(header))
                    continue;

                var rawValue = c < values.Count ? values[c] : string.Empty;
                row[header] = ParseCell(rawValue);
            }

            section.Rows.Add(row);
        }
    }

    private static List<SectionBlock> ParseBlocks(string content)
    {
        var result = new List<SectionBlock>();
        var headingPattern = new Regex(@"^\s*#{1,6}\s+(.+?)\s*$", RegexOptions.Compiled);

        string? typeName = null;
        var isDefault = false;
        List<string>? lines = null;

        using var reader = new StringReader(content);
        while (reader.ReadLine() is { } line)
        {
            var headingMatch = headingPattern.Match(line);
            if (headingMatch.Success)
            {
                if (typeName != null && lines != null)
                    result.Add(new SectionBlock(typeName, isDefault, lines));

                ParseHeader(headingMatch.Groups[1].Value, out typeName, out isDefault);
                lines = [];
                continue;
            }

            if (lines != null)
                lines.Add(line);
        }

        if (typeName != null && lines != null)
            result.Add(new SectionBlock(typeName, isDefault, lines));

        return result;
    }

    private static void ParseHeader(string rawHeader, out string typeName, out bool isDefault)
    {
        var header = rawHeader.Trim();
        var idx = header.IndexOf(':');
        if (idx < 0)
        {
            typeName = header;
            isDefault = false;
            return;
        }

        var suffix = header[(idx + 1)..].Trim();
        if (suffix.Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            typeName = header[..idx].Trim();
            isDefault = true;
            return;
        }

        typeName = header;
        isDefault = false;
    }

    private static List<string> ParseMarkdownRow(string line)
    {
        var value = line.Trim();
        if (value.StartsWith('|'))
            value = value[1..];

        if (value.EndsWith('|'))
            value = value[..^1];

        return value.Split('|').Select(cell => cell.Trim()).ToList();
    }

    private static bool IsSeparatorRow(IReadOnlyList<string> values)
    {
        if (values.Count == 0)
            return false;

        var seenDash = false;
        foreach (var cell in values)
        {
            foreach (var ch in cell)
            {
                if (ch == '-')
                    seenDash = true;

                if (ch != '-' && ch != ':' && !char.IsWhiteSpace(ch))
                    return false;
            }
        }

        return seenDash;
    }

    private static CellValue ParseCell(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return CellValue.Uninitialized;

        if (raw.Equals("@null", StringComparison.OrdinalIgnoreCase))
            return CellValue.ExplicitNull;

        return new CellValue(CellKind.Text, raw.Trim());
    }

    private static void AssignValue<T>(T item, string propertyName, CellValue cellValue,
        IReadOnlyDictionary<string, PropertyInfo> properties)
    {
        if (!properties.TryGetValue(propertyName, out var property))
            return;

        if (cellValue.Kind == CellKind.Uninitialized)
            return;

        if (cellValue.Kind == CellKind.ExplicitNull)
        {
            if (!CanBeNull(property.PropertyType))
                throw new InvalidOperationException(
                    $"Property {property.DeclaringType?.Name}.{property.Name} cannot be assigned null.");

            property.SetValue(item, null);
            return;
        }

        var converted = ConvertValue(cellValue.Value!, property.PropertyType);
        property.SetValue(item, converted);
    }

    private static object? ConvertValue(string token, Type targetType)
    {
        var nullableTarget = Nullable.GetUnderlyingType(targetType);
        var actualType = nullableTarget ?? targetType;

        var raw = ExpandMacro(token);
        if (raw == null)
            return null;

        if (actualType == typeof(string))
            return raw.ToString();

        if (actualType == typeof(Guid))
        {
            if (raw is Guid guid)
                return guid;

            return Guid.Parse(raw.ToString()!);
        }

        if (actualType == typeof(DateOnly))
        {
            if (raw is DateOnly dateOnly)
                return dateOnly;

            if (raw is DateTime dateTime)
                return DateOnly.FromDateTime(dateTime);

            return DateOnly.Parse(raw.ToString()!, CultureInfo.InvariantCulture);
        }

        if (actualType == typeof(DateTime))
        {
            if (raw is DateTime dateTime)
                return dateTime;

            return DateTime.Parse(raw.ToString()!, CultureInfo.InvariantCulture);
        }

        if (actualType.IsEnum)
            return Enum.Parse(actualType, raw.ToString()!, ignoreCase: true);

        var text = raw.ToString()!;
        if (actualType == typeof(bool))
            return bool.Parse(text);

        if (actualType == typeof(byte))
            return byte.Parse(text, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        if (actualType == typeof(short))
            return short.Parse(text, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        if (actualType == typeof(int))
            return int.Parse(text, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        if (actualType == typeof(long))
            return long.Parse(text, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        if (actualType == typeof(float))
            return float.Parse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        if (actualType == typeof(double))
            return double.Parse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        if (actualType == typeof(decimal))
            return decimal.Parse(text, NumberStyles.Number, CultureInfo.InvariantCulture);

        var converter = TypeDescriptor.GetConverter(actualType);
        if (converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFromInvariantString(text);

        return Convert.ChangeType(text, actualType, CultureInfo.InvariantCulture);
    }

    private static object ExpandMacro(string token)
    {
        return token.ToLowerInvariant() switch
        {
            "@empty" => string.Empty,
            "@today" => DateTime.Today,
            "@now" => DateTime.Now,
            "@utcnow" => DateTime.UtcNow,
            "@uuid" => Guid.NewGuid(),
            _ => token
        };
    }
}

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotNetCommons.Text.Csv;

public class CsvConverter<T>
    where T : class, new()
{
    private sealed class FieldDefinition
    {
        public required PropertyInfo PropInfo { get; init; }
        public required CsvFieldAttribute Attr { get; init; }
        public required string Format { get; init; }
        public required bool HasFormat { get; init; }
        public required Func<T, object?> Getter { get; init; }
        public required Action<T, object?> Setter { get; init; }
        public string Name => Attr.Name;
    }

    private static readonly FieldDefinition[] ClassProperties = BuildClassProperties();
    private static readonly Dictionary<string, FieldDefinition> PropertyMap =
        ClassProperties.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

    private readonly StringBuilder _field = new();
    private readonly StringBuilder _output = new();
    private readonly List<string> _fields = [];

    private List<FieldDefinition>? _readProperties;

    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
    public char? CommentCharacter { get; set; }
    public char Delimiter { get; set; } = ',';
    public char QuoteCharacter { get; set; } = '"';
    public string NewLine { get; set; } = Environment.NewLine;

    private static FieldDefinition[] BuildClassProperties()
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Select(p => (PropInfo: p, Attr: p.GetCustomAttribute<CsvFieldAttribute>()))
            .Where(p => p.Attr != null)
            .Select(p => CreateFieldDefinition(p.PropInfo, p.Attr!))
            .ToArray();
    }

    private static FieldDefinition CreateFieldDefinition(PropertyInfo property, CsvFieldAttribute attr)
    {
        var format = attr.Format ?? string.Empty;

        return new FieldDefinition
        {
            PropInfo = property,
            Attr = attr,
            Format = format,
            HasFormat = !string.IsNullOrEmpty(format),
            Getter = CreateGetter(property),
            Setter = CreateSetter(property),
        };
    }

    private static Func<T, object?> CreateGetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(T), "instance");
        var propertyAccess = Expression.Property(instance, property);
        var convert = Expression.Convert(propertyAccess, typeof(object));
        return Expression.Lambda<Func<T, object?>>(convert, instance).Compile();
    }

    private static Action<T, object?> CreateSetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(T), "instance");
        var value = Expression.Parameter(typeof(object), "value");
        var propertyAccess = Expression.Property(instance, property);
        var convertedValue = CreateSetterValueExpression(value, property.PropertyType);
        var assign = Expression.Assign(propertyAccess, convertedValue);
        return Expression.Lambda<Action<T, object?>>(assign, instance, value).Compile();
    }

    private static Expression CreateSetterValueExpression(Expression value, Type propertyType)
    {
        if (!propertyType.IsNullable())
            return Expression.Convert(value, propertyType);

        var valueType = Nullable.GetUnderlyingType(propertyType)!;
        var constructor = propertyType.GetConstructor([valueType])!;
        return Expression.Condition(
            Expression.Equal(value, Expression.Constant(null)),
            Expression.Default(propertyType),
            Expression.New(constructor, Expression.Convert(value, valueType)));
    }

    public string Convert(T item)
    {
        _output.Clear();
        for (var i = 0; i < ClassProperties.Length; i++)
        {
            if (i > 0)
                _output.Append(Delimiter);

            var field = ClassProperties[i];
            var value = field.Getter(item);
            AppendEscapedField(_output, FormatValue(value, field));
        }

        return _output.ToString();
    }

    private string FormatValue(object? value, FieldDefinition field)
    {
        if (value == null)
            return string.Empty;

        return field.HasFormat && value is IFormattable formattable
            ? formattable.ToString(field.Format, Culture)
            : value.ToString() ?? string.Empty;
    }

    public string ConvertHeader()
    {
        _output.Clear();
        for (var i = 0; i < ClassProperties.Length; i++)
        {
            if (i > 0)
                _output.Append(Delimiter);

            AppendEscapedField(_output, ClassProperties[i].Name);
        }

        return _output.ToString();
    }

    private void AppendEscapedField(StringBuilder output, string field)
    {
        if (string.IsNullOrEmpty(field))
            return;

        var needsQuoting = false;
        for (var i = 0; i < field.Length; i++)
        {
            var c = field[i];
            if (c == Delimiter || c == QuoteCharacter || c == '\n' || c == '\r')
            {
                needsQuoting = true;
                break;
            }
        }

        if (!needsQuoting)
        {
            output.Append(field);
            return;
        }

        output.Append(QuoteCharacter);
        for (var i = 0; i < field.Length; i++)
        {
            var c = field[i];
            if (c == QuoteCharacter)
                output.Append(QuoteCharacter);
            output.Append(c);
        }
        output.Append(QuoteCharacter);
    }

    public T Parse(string data)
    {
        if (_readProperties == null)
            throw new InvalidOperationException("ParseHeader must be called before Parse.");

        var fields = ParseFields(data);
        var item = new T();

        for (var i = 0; i < Math.Min(fields.Count, _readProperties.Count); i++)
        {
            var fieldValue = fields[i];
            if (fieldValue.IsSet())
                SetPropertyValue(item, _readProperties[i], fieldValue);
        }

        return item;
    }

    private void SetPropertyValue(T item, FieldDefinition field, string fieldValue)
    {
        field.Setter(item, field.PropInfo.ConvertPropertyValue(fieldValue, Culture, field.Format));
    }

    public void ParseHeader(string data)
    {
        var fields = ParseFields(data);

        _readProperties = [];
        foreach (var field in fields)
        {
            if (!PropertyMap.TryGetValue(field, out var propertyInfo))
                throw new InvalidOperationException($"Header field '{field}' does not match any property with CsvFieldAttribute.");

            _readProperties.Add(propertyInfo);
        }
    }

    private List<string> ParseFields(string data)
    {
        _fields.Clear();
        _field.Clear();
        var inQuotes = false;

        for (var i = 0; i < data.Length; i++)
        {
            var c = data[i];

            if (c == QuoteCharacter)
            {
                if (inQuotes && i + 1 < data.Length && data[i + 1] == QuoteCharacter)
                {
                    _field.Append(QuoteCharacter);
                    i++;
                }
                else
                    inQuotes = !inQuotes;
            }
            else if (c == Delimiter && !inQuotes)
            {
                _fields.Add(_field.ToString());
                _field.Clear();
            }
            else
                _field.Append(c);
        }

        _fields.Add(_field.ToString());
        return _fields;
    }

    public async IAsyncEnumerable<T> Read(StreamReader reader, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await ReadHeader(reader, ct);
        for(;;)
        {
            var item = await ReadOne(reader, ct);
            if (item != null)
                yield return item;
            else
                yield break;
        }
    }

    public async Task ReadHeader(StreamReader reader, CancellationToken ct = default)
    {
        for(;;)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line == null)
                break;

            line = line.Trim();
            if (line.Length == 0)
                continue;
            if (CommentCharacter.HasValue && line[0] == CommentCharacter.Value)
                continue;

            ParseHeader(line);
            break;
        }
    }

    public async Task<T?> ReadOne(StreamReader reader, CancellationToken ct = default)
    {
        var line = await reader.ReadLineAsync(ct);
        return line == null ? null : Parse(line);
    }

    public async Task Write(StreamWriter writer, IEnumerable<T> items, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await writer.WriteLineAsync(ConvertHeader());
        foreach (var item in items)
            await WriteOne(writer, item, ct);
    }

    public async Task WriteOne(StreamWriter writer, T item, CancellationToken ct = default)
    {
        var line = Convert(item);
        ct.ThrowIfCancellationRequested();
        await writer.WriteLineAsync(line);
    }
}

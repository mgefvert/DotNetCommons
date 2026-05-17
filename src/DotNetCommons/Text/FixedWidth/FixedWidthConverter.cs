using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotNetCommons.Text.FixedWidth;

public class FixedWidthConverter
{
    private sealed class PropInfo
    {
        public required PropertyInfo Prop { get; init; }
        public required FixedWidthAttribute Attr { get; init; }
        public required Type PropertyType { get; init; }
        public required Type ValueType { get; init; }
        public required bool IsNullable { get; init; }
        public required bool IsNonNullableNumeric { get; init; }
        public required Func<object, object?> Getter { get; init; }
        public required Action<object, object?> Setter { get; init; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int Length { get; set; }
    }

    private sealed class Definition
    {
        public required PropInfo[] Fields { get; init; }
        public required int MaxLength { get; init; }
    }

    private static class DefinitionCache<T>
    {
        public static readonly Definition Value = BuildDefinition(typeof(T));
    }

    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    private static Definition BuildDefinition(Type type)
    {
        var fields = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => (Prop: p, Attr: p.GetCustomAttribute<FixedWidthAttribute>()))
            .Where(p => p.Attr != null)
            .Select(p => CreatePropInfo(p.Prop, p.Attr!))
            .OrderBy(p => p.Attr.Start)
            .ToArray();

        foreach (var f in fields)
        {
            f.StartIndex = f.Attr.Start - 1;
            f.Length = f.Attr.Length;
            f.EndIndex = f.StartIndex + f.Length;
        }

        for (var i = 1; i < fields.Length; i++)
            if (fields[i].StartIndex < fields[i - 1].EndIndex)
                throw new InvalidDataException($"Fixed width definitions for class {type.Name} overlap in position {fields[i].Attr.Start}");

        if (fields.Length == 0)
            throw new InvalidDataException($"Class {type.Name} has no FixedWidth definitions");

        return new Definition
        {
            Fields = fields,
            MaxLength = fields[^1].EndIndex,
        };
    }

    private static PropInfo CreatePropInfo(PropertyInfo property, FixedWidthAttribute attr)
    {
        var propertyType = property.PropertyType;
        var isNullable = propertyType.IsNullable();
        var valueType = isNullable ? Nullable.GetUnderlyingType(propertyType)! : propertyType;

        return new PropInfo
        {
            Prop = property,
            Attr = attr,
            PropertyType = propertyType,
            ValueType = valueType,
            IsNullable = isNullable,
            IsNonNullableNumeric = valueType.IsNumeric() && !isNullable,
            Getter = CreateGetter(property),
            Setter = CreateSetter(property),
        };
    }

    private static Func<object, object?> CreateGetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var typedInstance = Expression.Convert(instance, property.DeclaringType!);
        var propertyAccess = Expression.Property(typedInstance, property);
        var convert = Expression.Convert(propertyAccess, typeof(object));
        return Expression.Lambda<Func<object, object?>>(convert, instance).Compile();
    }

    private static Action<object, object?> CreateSetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");
        var typedInstance = Expression.Convert(instance, property.DeclaringType!);
        var propertyAccess = Expression.Property(typedInstance, property);
        var convertedValue = CreateSetterValueExpression(value, property.PropertyType);
        var assign = Expression.Assign(propertyAccess, convertedValue);
        return Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();
    }

    private static Expression CreateSetterValueExpression(Expression value, Type propertyType)
    {
        if (!propertyType.IsNullable())
            return Expression.Convert(value, propertyType);

        var valueType = Nullable.GetUnderlyingType(propertyType)!;
        var constructor = propertyType.GetConstructor([valueType])!;
        return Expression.New(constructor, Expression.Convert(value, valueType));
    }

    public string Convert<T>(T item)
    {
        var definition = GetDefinition<T>();
        var info = definition.Fields;
        var result = new char[definition.MaxLength];
        Array.Fill(result, ' ');

        foreach (var f in info)
        {
            var value = f.Getter(item!);
            var s = f.Attr.FormatValue(value, Culture);

            if (s.Length != f.Length)
                throw new InvalidDataException($"Invalid data length returned from formatter for value '{value}' => '{s}', " +
                                               $"expected length {f.Length} but got {s.Length}.");

            s.CopyTo(0, result, f.StartIndex, f.Length);
        }

        return new string(result);
    }

    public T Parse<T>(string data)
        where T : class, new()
    {
        var info = GetDefinition<T>().Fields;
        var result = new T();

        foreach (var f in info)
        {
            var s = GetFieldData(data, f);
            var value = f.Attr.Parse(s, Culture);

            if (value == null && f.IsNonNullableNumeric)
                value = 0;

            try
            {
                f.Setter(result, ConvertParsedValue(value, f));
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Data conversion failed for field {typeof(T).Name}.{f.Prop.Name}, data='{s}'.", e);
            }
        }

        return result;
    }

    private static Definition GetDefinition<T>()
    {
        try
        {
            return DefinitionCache<T>.Value;
        }
        catch (TypeInitializationException e) when (e.InnerException is InvalidDataException invalidDataException)
        {
            throw invalidDataException;
        }
    }

    private static string GetFieldData(string data, PropInfo f)
    {
        if (f.StartIndex >= data.Length)
            return new string(' ', f.Length);

        if (data.Length >= f.EndIndex)
            return data.Substring(f.StartIndex, f.Length);

        return data.Substring(f.StartIndex).PadRight(f.Length);
    }

    private static object? ConvertParsedValue(object? value, PropInfo f)
    {
        if (value == null)
            return f.IsNullable || !f.ValueType.IsValueType ? null : Activator.CreateInstance(f.ValueType);

        var valueType = value.GetType();
        if (valueType == f.ValueType || valueType.DescendantOfOrEqual(f.ValueType))
            return value;

        if (f.ValueType == typeof(DateOnly) && value is DateTime dt)
            return DateOnly.FromDateTime(dt);
        if (f.ValueType == typeof(bool))
            return value.ToString().ParseBoolean(false);
        if (f.ValueType == typeof(Guid))
            return Guid.Parse(value.ToString()!);
        if (f.ValueType == typeof(Uri))
            return new Uri(value.ToString()!);
        if (f.ValueType.IsEnum)
            return Enum.Parse(f.ValueType, value.ToString()!.Replace("-", ""), true);

        return System.Convert.ChangeType(value, f.ValueType, CultureInfo.InvariantCulture);
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

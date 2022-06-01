#nullable disable
using DotNetCommons.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Parsers;

[AttributeUsage(AttributeTargets.Property)]
public class CsvFieldAttribute : Attribute
{
    public string Name { get; }
    public bool Required { get; }

    public CsvFieldAttribute(string name, bool required)
    {
        Name = name;
        Required = required;
    }
}

public class CsvFieldDefinition
{
    public CsvFieldAttribute Attribute { get; }
    public int FieldNo { get; set; }
    public PropertyInfo Property { get; }

    public CsvFieldDefinition(CsvFieldAttribute attribute, PropertyInfo property)
    {
        Attribute = attribute;
        Property = property;
        FieldNo = -1;
    }
}

public enum InvalidDataReason
{
    RequiredValueMissing,
    ConversionFailed
}

public class InvalidDataArgs : EventArgs
{
    public Exception Exception { get; }
    public int? LineNo { get; }
    public InvalidDataReason Reason { get; }

    public InvalidDataArgs(int? lineNo, InvalidDataReason reason, Exception exception)
    {
        LineNo = lineNo;
        Reason = reason;
        Exception = exception;
    }
}

public class PreProcessFieldArgs : EventArgs
{
    public string Data { get; set; }
    public bool Discard { get; set; }
    public PropertyInfo Property { get; }

    public PreProcessFieldArgs(PropertyInfo property, string data)
    {
        Property = property;
        Data = data;
    }
}

public class PreProcessRowArgs : EventArgs
{
    public bool Discard { get; set; }
    public List<string> Fields { get; set; }

    public PreProcessRowArgs(List<string> fields)
    {
        Fields = fields;
    }
}

public class PostProcessObjectArgs<T> : EventArgs
{
    public bool Discard { get; set; }
    public T Object { get; set; }

    public PostProcessObjectArgs(T obj)
    {
        Object = obj;
    }
}

public delegate void InvalidDataDelegate(object sender, InvalidDataArgs args);
public delegate void PreProcessFieldDelegate(object sender, PreProcessFieldArgs args);
public delegate void PreProcessRowDelegate(object sender, PreProcessRowArgs args);
public delegate void PostProcessObjectDelegate<T>(object sender, PostProcessObjectArgs<T> args);

public class CsvParser<T> where T : class, new()
{
    protected readonly List<CsvFieldDefinition> Definitions = new();
    private bool _gotHeaders;
    public CsvParser Parser { get; } = new();
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    public event InvalidDataDelegate InvalidData;
    public event PreProcessFieldDelegate PreProcessField;
    public event PreProcessRowDelegate PreProcessRow;
    public event PostProcessObjectDelegate<T> PostProcessObject;

    public CsvParser()
    {
        Definitions.AddRange(ProcessClass(typeof(T)));
    }

    protected void FireInvalidData(int? lineNo, InvalidDataReason reason, Exception exception)
    {
        if (InvalidData != null)
            InvalidData(this, new InvalidDataArgs(lineNo, reason, exception));
        else
            throw new InvalidDataException($"Invalid data in CSV parsing ({reason}) on line {lineNo}: {exception.Message}");
    }

    protected bool FirePreProcessField(CsvFieldDefinition definition, ref string data)
    {
        if (PreProcessField == null)
            return false;

        var args = new PreProcessFieldArgs(definition.Property, data);
        PreProcessField(this, args);

        data = args.Data;
        return args.Discard;
    }

    protected bool FirePreProcessRow(List<string> fields)
    {
        if (PreProcessRow == null)
            return false;

        var args = new PreProcessRowArgs(fields);
        PreProcessRow(this, args);
        return args.Discard;
    }

    protected bool FirePostProcessRow(T obj)
    {
        if (PostProcessObject == null)
            return false;

        var args = new PostProcessObjectArgs<T>(obj);
        PostProcessObject(this, args);
        return args.Discard;
    }

    internal static IEnumerable<CsvFieldDefinition> ProcessClass(Type type)
    {
        return
            from property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            let attr = property.GetCustomAttribute<CsvFieldAttribute>()
            where attr != null
            select new CsvFieldDefinition(attr, property);
    }

    public void ProcessHeaders(List<string> fields)
    {
        var lookup = new Dictionary<string, int>();
        for (int i = 0; i < fields.Count; i++)
            lookup[fields[i].ToLower()] = i;

        foreach (var definition in Definitions)
        {
            var name = definition.Attribute.Name.ToLower();
            if (lookup.TryGetValue(name, out var value))
                definition.FieldNo = value;
        }

        var missing = Definitions.Where(x => x.Attribute.Required && x.FieldNo == -1).ToList();
        if (missing.Any())
            throw new CsvException("Missing fields in CSV header: " + string.Join(", ", missing));

        _gotHeaders = true;
    }

    public void ProcessHeaders(string text)
    {
        var strings = Parser.ParseRow(text);
        ProcessHeaders(strings);
    }

    public T ProcessLine(string text, int? lineNo = null)
    {
        var strings = Parser.ParseRow(text);
        return StringsToObject(strings, lineNo);
    }

    public List<T> ProcessLines(string text)
    {
        var rows = Parser.ParseRows(text);
        if (!rows.Any())
            return new List<T>();

        int offset = 0;
        if (!_gotHeaders)
        {
            var header = rows.ExtractFirst();
            ProcessHeaders(header);
            offset++;
        }

        return rows
            .Select((x, i) => StringsToObject(x, i + offset))
            .Where(x => x != null)
            .ToList();
    }

    private T StringsToObject(List<string> strings, int? lineNo)
    {
        if (string.IsNullOrEmpty(string.Join("", strings).Trim()))
            return null;

        if (FirePreProcessRow(strings))
            return null;

        var obj = new T();
        foreach (var definition in Definitions)
        {
            if (definition.FieldNo == -1)
                continue;

            var value = strings.ElementAtOrDefault(definition.FieldNo);
            if (FirePreProcessField(definition, ref value))
                continue;

            if (definition.Attribute.Required && string.IsNullOrWhiteSpace(value))
            {
                FireInvalidData(lineNo, InvalidDataReason.RequiredValueMissing, null);
                continue;
            }

            try
            {
                definition.Property.SetPropertyValue(obj, value, Culture);
            }
            catch (Exception ex)
            {
                FireInvalidData(lineNo, InvalidDataReason.ConversionFailed, ex);
            }
        }

        return FirePostProcessRow(obj) ? null : obj;
    }
}
﻿using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DotNetCommons.Collections;

public class GridRenderOptions<TRow, TCol, TData>
{
    public bool IncludeColumnHeader { get; set; }
    public bool IncludeRowHeader { get; set; }
    public bool SortColumns { get; set; }
    public bool SortRows { get; set; }

    public char CsvSeparator { get; set; } = ',';
    public string CsvLineSeparator { get; set; } = Environment.NewLine;

    public Func<TRow, string?>? RowFormatter { get; set; }
    public Func<TCol, string?>? ColumnFormatter { get; set; }
    public Func<TData?, string?>? ValueFormatter { get; set; }
}

public static class Grid
{
    /// <summary>
    /// Transform a number of objects into a grid, using a key selector to select the key (row) for
    /// each object, transforming each property into a string value.
    /// </summary>
    /// <param name="items">A list of objects to transform into a grid.</param>
    /// <param name="keySelector">The selector for the key value which becomes the row identity.</param>
    /// <returns>A grid populated with string values.</returns>
    public static Grid<TKey, string, string?> BuildFromObjects<TObject, TKey>(IEnumerable<TObject> items, Func<TObject, TKey> keySelector)
        where TKey : notnull
        where TObject : class
    {
        return BuildFromObjects(items, keySelector, (prop,  value) => value?.ToString());
    }

    /// <summary>
    /// Transform a number of objects into a grid, using a key selector to select the key (row) for
    /// each object and a value transformer to format the values into the grid data type.
    /// </summary>
    /// <param name="items">A list of objects to transform into a grid.</param>
    /// <param name="keySelector">The selector for the key value which becomes the row identity.</param>
    /// <param name="valueTransform">A delegate that transforms the value in a given field to the grid data type, e.g.
    ///     runs a ToString() to turn all the values into strings. If the value returns null, nothing will be inserted
    ///     into the grid and can be used to hide fields entirely.</param>
    /// <returns>A grid populated with values.</returns>
    public static Grid<TKey, string, TValue> BuildFromObjects<TObject, TKey, TValue>(
            IEnumerable<TObject> items, Func<TObject, TKey> keySelector, Func<PropertyInfo, object?, TValue> valueTransform)
        where TKey : notnull
        where TObject : class
    {
        var result = new Grid<TKey, string, TValue>();

        var fields = typeof(TObject).GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var item in items)
        {
            var key = keySelector(item);

            foreach (var field in fields)
            {
                var fieldValue = field.GetValue(item);
                var value = valueTransform(field, fieldValue);

                result[key, field.Name] = value;
            }
        }

        return result;
    }
}

/// <summary>
/// Class that allows you to store values in a grid using any given values for rows or columns and
/// generate CSV files, MarkDown tables or HTML tables. Uses a dictionary internally with (row, column)
/// tuples to efficiently store data.
/// </summary>
/// <typeparam name="TRow">Any generic type for row definitions.</typeparam>
/// <typeparam name="TCol">Any generic type for column definitions.</typeparam>
/// <typeparam name="TData">Any generic type for data elements.</typeparam>
public class Grid<TRow, TCol, TData>
    where TRow : notnull
    where TCol : notnull
{
    public class Element<T>
    {
        public T Header { get; }
        public TData? Value { get; }

        public Element(T header, TData? value)
        {
            Header = header;
            Value = value;
        }
    }

    public class EqualityComparer : IEqualityComparer<(TRow, TCol)>
    {
        private readonly IEqualityComparer<TRow> _rowComparer;
        private readonly IEqualityComparer<TCol> _columnComparer;

        public EqualityComparer(IEqualityComparer<TRow> rowComparer, IEqualityComparer<TCol> columnComparer)
        {
            _rowComparer = rowComparer;
            _columnComparer = columnComparer;
        }

        public bool Equals((TRow, TCol) x, (TRow, TCol) y)
        {
            return _rowComparer.Equals(x.Item1, y.Item1) && 
                   _columnComparer.Equals(x.Item2, y.Item2);
        }

        public int GetHashCode((TRow, TCol) obj)
        {
            return HashCode.Combine(_rowComparer.GetHashCode(obj.Item1), _columnComparer.GetHashCode(obj.Item2));
        }
    }

    private readonly char[] _csvEscapeChars = ['"'];
    private readonly char[] _markupEscapeChars = ['|'];

    /// <summary>
    /// The defined Columns in the grid. Are added to automatically as data is added, but can be
    /// used to add additional fields beforehand to guarantee sort order.
    /// </summary>
    public HashSet<TCol> Columns { get; }

    /// <summary>
    /// The defined Rows in the grid. Are added to automatically as data is added, but can be
    /// modified manually.
    /// </summary>
    public HashSet<TRow> Rows { get; }

    /// <summary>
    /// Data elements. Use [row, col] syntax to access these efficiently.
    /// </summary>
    public Dictionary<(TRow, TCol), TData?> Data { get; }

    protected (TRow, TCol) Key(TRow row, TCol column) => new(row, column);

    public Grid()
    {
        Data    = new Dictionary<(TRow, TCol), TData?>();
        Columns = [];
        Rows    = [];
    }

    public Grid(IEqualityComparer<TRow> rowComparer, IEqualityComparer<TCol> columnComparer)
    {
        Data = new Dictionary<(TRow, TCol), TData?>(new EqualityComparer(rowComparer, columnComparer));
        Columns = new HashSet<TCol>(columnComparer);
        Rows = new HashSet<TRow>(rowComparer);
    }

    public void Clear()
    {
        Columns.Clear();
        Rows.Clear();
        Data.Clear();
    }

    public bool Exists(TRow row, TCol column)
    {
        var key = Key(row, column);
        return Data.ContainsKey(key);
    }

    /// <summary>
    /// Extract a given row/column item from the grid, removing it.
    /// </summary>
    public TData? Extract(TRow row, TCol column, TData? defaultValue = default)
    {
        var key = Key(row, column);
        var result = Get(key, defaultValue);
        Data.Remove(key);
        return result;
    }

    protected TData? Get((TRow, TCol) key, TData? defaultValue)
    {
        return Data.TryGetValue(key, out var result)
            ? result
            : defaultValue;
    }

    /// <summary>
    /// Get a given row/column item from the grid.
    /// </summary>
    public TData? Get(TRow row, TCol column, TData? defaultValue = default)
    {
        return Get(Key(row, column), defaultValue);
    }

    /// <summary>
    /// Return all columns and their data values for a given row.
    /// </summary>
    public IEnumerable<Element<TCol>> GetColumnsForRow(TRow row)
    {
        foreach (var col in Columns)
            yield return new Element<TCol>(col, Get(row, col));
    }

    /// <summary>
    /// Return all rows and their data values for a given column.
    /// </summary>
    public IEnumerable<Element<TRow>> GetRowsForColumn(TCol column)
    {
        foreach (var row in Rows)
            yield return new Element<TRow>(row, Get(row, column));
    }

    public Grid<TCol, TRow, TData> Invert()
    {
        var result = new Grid<TCol, TRow, TData>();

        result.Columns.AddRange(Rows);
        result.Rows.AddRange(Columns);

        foreach (var ((row, col), value) in Data)
            result.Data[result.Key(col, row)] = value;

        return result;
    }

    /// <summary>
    /// Callback to manipulate the value of a given row/column item; main use is to cut down on
    /// allocations for multiple row/column pairs.
    /// </summary>
    public TData? Manipulate(TRow row, TCol column, Func<TData?, TData?> func, TData? defaultValue = default)
    {
        var key = Key(row, column);
        var value = func(Get(key, defaultValue));
        return Set(key, value);
    }

    /// <summary>
    /// Callback to manipulate the value of a given row/column item; main use is to cut down on
    /// allocations for multiple row/column pairs.
    /// </summary>
    public void Manipulate(Func<(TRow, TCol), bool> selector, Func<TData?, TData?> func)
    {
        var selection = Data.Keys.Where(selector).ToList();
        foreach (var item in selection)
        {
            var data = Data[item];
            Data[item] = func(data);
        }
    }

    public void RemoveValues(Func<TRow, TCol, TData?, bool> selector)
    {
        var keys = Data
            .Where(x => selector(x.Key.Item1, x.Key.Item2, x.Value))
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keys)
            Data.Remove(key);
    }

    public void RemoveColumn(TCol col)
    {
        Columns.Remove(col);
        var keys = Data
            .Where(x => x.Key.Item2.Equals(col))
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keys)
            Data.Remove(key);
    }

    public void RemoveEmptyColumns()
    {
        var existingColumns = Data
            .Select(x => x.Key.Item2)
            .Distinct()
            .ToHashSet();

        Columns.RemoveWhere(c => !existingColumns.Contains(c));
    }

    public void RemoveEmptyRows()
    {
        var existingRows = Data
            .Select(x => x.Key.Item1)
            .Distinct()
            .ToHashSet();

        Rows.RemoveWhere(r => !existingRows.Contains(r));
    }

    public void RemoveRow(TRow row)
    {
        Rows.Remove(row);
        var keys = Data
            .Where(x => x.Key.Item1.Equals(row))
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keys)
            Data.Remove(key);
    }

    protected TData? Set((TRow, TCol) key, TData? value)
    {
        if (!Columns.Contains(key.Item2))
            Columns.Add(key.Item2);
        if (!Rows.Contains(key.Item1))
            Rows.Add(key.Item1);

        Data[key] = value;
        return value;
    }

    /// <summary>
    /// Set a row/column item to a given value.
    /// </summary>
    public TData? Set(TRow row, TCol column, TData? value)
    {
        return Set(Key(row, column), value);
    }

    /// <summary>
    /// Efficient accessor for get/set operations.
    /// </summary>
    public TData? this[TRow row, TCol col]
    {
        get => Get(row, col);
        set => Set(row, col, value);
    }


    // --- EXPORT FUNCTIONS -----------------------------------------------

    private static bool IsNumeric(string? result)
    {
        return double.TryParse(result, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out _);
    }

    private static string EscapeString(string? value, char[] escapeChars, bool addQuotes)
    {
        var result = value ?? "";
        foreach (var ch in escapeChars)
            result = result.Replace(ch.ToString(), "\\" + ch);

        result = result
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");

        if (addQuotes && !IsNumeric(result))
            result = '"' + result + '"';

        return result;
    }

    private List<List<string?>> RenderToStrings(GridRenderOptions<TRow, TCol, TData> options, Func<string?, string?> formatter)
    {
        var columns = options.SortColumns ? Columns.OrderBy(x => x).ToList() : Columns.ToList();
        var rows = options.SortRows ? Rows.OrderBy(x => x).ToList() : Rows.ToList();

        var result = new List<List<string?>>(rows.Count + 1);

        if (options.IncludeColumnHeader)
        {
            var header = new List<string?>(columns.Count + 1);
            if (options.IncludeRowHeader)
                header.Add(formatter("Row"));
            header.AddRange(columns.Select(x => formatter(x.ToString())));
            result.Add(header);
        }

        foreach (var row in rows)
        {
            var strings = new List<string?>(columns.Count + 1);
            if (options.IncludeRowHeader)
                strings.Add(formatter(row.ToString()));
            strings.AddRange(columns.Select(c => formatter(Get(row, c)?.ToString())));
            result.Add(strings);
        }

        return result;
    }

    /// <summary>
    /// Generate a CSV string from the grid.
    /// </summary>
    public string? ToCsv(GridRenderOptions<TRow, TCol, TData> options)
    {
        if (!Columns.Any() || !Rows.Any())
            return null;

        var strings = RenderToStrings(options, v => EscapeString(v, _csvEscapeChars, true));

        var result = new StringBuilder();
        foreach (var row in strings)
            result.Append(string.Join(options.CsvSeparator, row) + options.CsvLineSeparator);

        return result.ToString();
    }

    /// <summary>
    /// Render grid to HTML.
    /// </summary>
    public string? ToHtml(GridRenderOptions<TRow, TCol, TData> options)
    {
        if (!Columns.Any() || !Rows.Any())
            return null;

        var rowFormatter = options.RowFormatter ?? (row => row.ToString());
        var columnFormatter = options.ColumnFormatter ?? (col => col.ToString());
        var valueFormatter = options.ValueFormatter ?? (v => v?.ToString());

        var columns = options.SortColumns ? Columns.OrderBy(x => x).ToList() : Columns.ToList();
        var rows = options.SortRows ? Rows.OrderBy(x => x).ToList() : Rows.ToList();

        var result = new StringBuilder();
        result.AppendLine("<table>");
        result.AppendLine("  <thead>");
        result.AppendLine("    <tr>");
        result.AppendLine("      <th>Row</th>");
        foreach (var col in columns)
            result.AppendLine("      <th>" + WebUtility.HtmlEncode(columnFormatter(col)) + "</th>");
        result.AppendLine("    </tr>");
        result.AppendLine("  </thead>");
        result.AppendLine("  <tbody>");

        foreach (var row in rows)
        {
            result.AppendLine("    <tr>");
            result.AppendLine("      <th>" + WebUtility.HtmlEncode(rowFormatter(row)) + "</th>");
            foreach (var col in columns)
                result.AppendLine("      <td>" + WebUtility.HtmlEncode(valueFormatter(Get(row, col))) + "</td>");
            result.AppendLine("    </tr>");
        }

        result.AppendLine("  </tbody>");
        result.AppendLine("</table>");
        return result.ToString();
    }

    /// <summary>
    /// Generate a MarkDown table (fields separated by pipe symbol).
    /// </summary>
    /// <returns></returns>
    public string? ToMarkDown(GridRenderOptions<TRow, TCol, TData> options)
    {
        if (!Columns.Any() || !Rows.Any())
            return null;

        var strings = RenderToStrings(options, v => EscapeString(v, _markupEscapeChars, false));
        var colCount = strings.First().Count;
        var colLengths = Enumerable.Range(0, colCount)
            .Select(col => strings.Select(s => s[col]?.Length ?? 0).Max())
            .ToList();

        var result = new StringBuilder();

        void GenerateLine(IEnumerable<string?> columns, string separator) =>
            result.AppendLine(string.Join(separator, columns.Select((s, index) => s?.PadRight(colLengths[index]))));

        GenerateLine(strings.First(), " | ");
        GenerateLine(colLengths.Select(x => new string('-', x)), "-|-");

        foreach (var row in strings.Skip(1))
            GenerateLine(row, " | ");

        return result.ToString();
    }
}
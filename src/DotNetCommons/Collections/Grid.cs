using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections
{
    /// <summary>
    /// Class that allows you to store values in a grid using any given values for rows or columns and
    /// generate CSV files, MarkDown tables or HTML tables. Uses a dictionary internally with (row, column)
    /// tuples to efficiently store data.
    /// </summary>
    /// <typeparam name="TRow">Any generic type for row definitions.</typeparam>
    /// <typeparam name="TCol">Any generic type for column definitions.</typeparam>
    /// <typeparam name="TData">Any generic type for data elements.</typeparam>
    public class Grid<TRow, TCol, TData>
    {
        public class Element<T>
        {
            public T Header { get; set; }
            public TData Value { get; set; }
        }

        private readonly char[] _csvEscapeChars = { '"' };
        private readonly char[] _markupEscapeChars = { '|' };

        /// <summary>
        /// The defined Columns in the grid. Are added to automatically as data is added, but can be
        /// used to add additional fields beforehand to guarantee sort order.
        /// </summary>
        public List<TCol> Columns { get; } = new();

        /// <summary>
        /// The defined Rows in the grid. Are added to automatically as data is added, but can be
        /// modified manually.
        /// </summary>
        public List<TRow> Rows { get; } = new();

        /// <summary>
        /// Data elements. Use [row, col] syntax to access these efficiently.
        /// </summary>
        public Dictionary<Tuple<TRow, TCol>, TData> Data { get; } = new();

        protected Tuple<TRow, TCol> Key(TRow row, TCol column) => new(row, column);

        /// <summary>
        /// Extract a given row/column item from the grid, removing it.
        /// </summary>
        public TData Extract(TRow row, TCol column, TData defaultValue = default)
        {
            var key = Key(row, column);
            var result = Get(key, defaultValue);
            Data.Remove(key);
            return result;
        }

        protected TData Get(Tuple<TRow, TCol> key, TData defaultValue)
        {
            return Data.TryGetValue(key, out var result)
                ? result
                : defaultValue;
        }

        /// <summary>
        /// Get a given row/column item from the grid.
        /// </summary>
        public TData Get(TRow row, TCol column, TData defaultValue = default)
        {
            return Get(Key(row, column), defaultValue);
        }

        /// <summary>
        /// Return all columns and their data values for a given row.
        /// </summary>
        public IEnumerable<Element<TCol>> GetColumnsForRow(TRow row)
        {
            foreach (var col in Columns)
                yield return new Element<TCol> { Header = col, Value = Get(row, col) };
        }

        /// <summary>
        /// Return all rows and their data values for a given column.
        /// </summary>
        public IEnumerable<Element<TRow>> GetRowsForColumn(TCol column)
        {
            foreach (var row in Rows)
                yield return new Element<TRow> { Header = row, Value = Get(row, column) };
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
        public TData Manipulate(TRow row, TCol column, Func<TData, TData> func, TData defaultValue = default)
        {
            var key = Key(row, column);
            var value = func(Get(key, defaultValue));
            return Set(key, value);
        }

        protected TData Set(Tuple<TRow, TCol> key, TData value)
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
        public TData Set(TRow row, TCol column, TData value)
        {
            return Set(Key(row, column), value);
        }

        /// <summary>
        /// Efficient accessor for get/set operations.
        /// </summary>
        public TData this[TRow row, TCol col]
        {
            get => Get(row, col);
            set => Set(row, col, value);
        }


        // --- EXPORT FUNCTIONS -----------------------------------------------

        private bool IsNumeric(string result)
        {
            return double.TryParse(result, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out _);
        }

        private string EscapeString(string value, char[] escapeChars, bool addQuotes)
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

        private List<List<string>> RenderToStrings(bool rowHeader, bool colHeader, Func<string, string> formatter)
        {
            var result = new List<List<string>>(Rows.Count + 1);

            if (colHeader)
            {
                var header = new List<string>(Columns.Count + 1);
                if (rowHeader)
                    header.Add(formatter("Row"));
                header.AddRange(Columns.Select(x => formatter(x.ToString())));
                result.Add(header);
            }

            foreach (var row in Rows)
            {
                var strings = new List<string>(Columns.Count + 1);
                if (rowHeader)
                    strings.Add(formatter(row.ToString()));
                strings.AddRange(Columns.Select(c => formatter(Get(row, c)?.ToString())));
                result.Add(strings);
            }

            return result;
        }

        /// <summary>
        /// Generate a CSV string from the grid.
        /// </summary>
        public string ToCsv(char separator = ',', bool includeRowHeader = false)
        {
            if (!Columns.Any() || !Rows.Any())
                return null;

            var strings = RenderToStrings(includeRowHeader, true, v => EscapeString(v, _csvEscapeChars, true));

            var result = new StringBuilder();
            foreach (var row in strings)
                result.AppendLine(string.Join(separator, row));

            return result.ToString();
        }

        protected string RenderHtml(Func<TRow, string> rowFormatter, Func<TCol, string> columnFormatter, Func<TData, string> valueFormatter)
        {
            if (!Columns.Any() || !Rows.Any())
                return null;

            rowFormatter ??= row => row.ToString();
            columnFormatter ??= col => col.ToString();
            valueFormatter ??= v => v.ToString();

            var result = new StringBuilder();
            result.AppendLine("<table>");
            result.AppendLine("  <thead>");
            result.AppendLine("    <tr>");
            result.AppendLine("      <th>Row</th>");
            foreach (var col in Columns)
                result.AppendLine("      <th>" + WebUtility.HtmlEncode(columnFormatter(col)) + "</th>");
            result.AppendLine("    </tr>");
            result.AppendLine("  </thead>");
            result.AppendLine("  <tbody>");

            foreach (var row in Rows)
            {
                result.AppendLine("    <tr>");
                result.AppendLine("      <th>" + WebUtility.HtmlEncode(rowFormatter(row)) + "</th>");
                foreach (var col in Columns)
                    result.AppendLine("      <td>" + WebUtility.HtmlEncode(valueFormatter(Get(row, col))) + "</td>");
                result.AppendLine("    </tr>");
            }

            result.AppendLine("  </tbody>");
            result.AppendLine("</table>");
            return result.ToString();
        }

        /// <summary>
        /// Render grid to HTML.
        /// </summary>
        public string ToHtml()
        {
            return RenderHtml(null, null, null);
        }

        /// <summary>
        /// Render grid to HTML using a specific value formatter.
        /// </summary>
        public string ToHtml(Func<TData, string> valueFormatter)
        {
            return RenderHtml(null, null, valueFormatter);
        }

        public string ToHtml(Func<TRow, string> rowFormatter, Func<TCol, string> columnFormatter, Func<TData, string> valueFormatter)
        {
            return RenderHtml(rowFormatter, columnFormatter, valueFormatter);
        }

        /// <summary>
        /// Generate a MarkDown table (fields separated by pipe symbol).
        /// </summary>
        /// <returns></returns>
        public string ToMarkup(bool includeRowHeader)
        {
            if (!Columns.Any() || !Rows.Any())
                return null;

            var strings = RenderToStrings(includeRowHeader, true, v => EscapeString(v, _markupEscapeChars, false));
            var colCount = strings.First().Count;
            var colLengths = Enumerable.Range(0, colCount)
                .Select(col => strings.Select(s => s[col]?.Length ?? 0).Max())
                .ToList();

            var result = new StringBuilder();

            void GenerateLine(IEnumerable<string> columns, string separator) =>
                result.AppendLine(string.Join(separator, columns.Select((s, index) => s.PadRight(colLengths[index]))));

            GenerateLine(strings.First(), " | ");
            GenerateLine(colLengths.Select(x => new string('-', x)), "-|-");

            foreach (var row in strings.Skip(1))
                GenerateLine(row, " | ");

            return result.ToString();
        }
    }
}

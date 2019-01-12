using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections
{
    public class GridElement<T, TData>
    {
        public T Header { get; set; }
        public TData Value { get; set; }
    }

    public class Grid<TRow, TCol, TData> 
    {
        public List<TCol> Columns { get; } = new List<TCol>();
        public List<TRow> Rows { get; } = new List<TRow>();

        public Dictionary<Tuple<TRow, TCol>, TData> Data { get; } = new Dictionary<Tuple<TRow, TCol>, TData>();
        protected Tuple<TRow, TCol> Key(TRow row, TCol column) => new Tuple<TRow, TCol>(row, column);

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

        public TData Get(TRow row, TCol column, TData defaultValue = default)
        {
            return Get(Key(row, column), defaultValue);
        }

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

        public TData Set(TRow row, TCol column, TData value)
        {
            return Set(Key(row, column), value);
        }

        public TData this[TRow row, TCol col]
        {
            get => Get(row, col);
            set => Set(row, col, value);
        }

        public string ToCsv()
        {
            if (!Columns.Any() || !Rows.Any())
                return null;

            var result = new StringBuilder();
            result.AppendLine("Row," + string.Join(",", Columns));
            foreach (var row in Rows)
            {
                result.AppendLine(row + "," + string.Join(",", Columns.Select(c => Get(row, c).ToString())));
            }

            return result.ToString();
        }

        protected string RenderHtml(Func<TRow, string> rowFormatter, Func<TCol, string> columnFormatter, Func<TData, string> valueFormatter)
        {
            if (!Columns.Any() || !Rows.Any())
                return null;

            if (rowFormatter == null)
                rowFormatter = row => row.ToString();
            if (columnFormatter == null)
                columnFormatter = col => col.ToString();
            if (valueFormatter == null)
                valueFormatter = v => v.ToString();

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
                foreach(var col in Columns)
                    result.AppendLine("      <td>" + WebUtility.HtmlEncode(valueFormatter(Get(row, col))) + "</td>");
                result.AppendLine("    </tr>");
            }

            result.AppendLine("  </tbody>");
            result.AppendLine("</table>");
            return result.ToString();
        }

        public string ToHtml()
        {
            return RenderHtml(null, null, null);
        }

        public string ToHtml(Func<TData, string> valueFormatter)
        {
            return RenderHtml(null, null, valueFormatter);
        }

        public string ToHtml(Func<TRow, string> rowFormatter, Func<TCol, string> columnFormatter, Func<TData, string> valueFormatter)
        {
            return RenderHtml(rowFormatter, columnFormatter, valueFormatter);
        }

        public IEnumerable<GridElement<TCol, TData>> GetColumnsForRow(TRow row)
        {
            foreach (var col in Columns)
                yield return new GridElement<TCol, TData> { Header = col, Value = Get(row, col) };
        }

        public IEnumerable<GridElement<TRow, TData>> GetRowsForColumn(TCol column)
        {
            foreach (var row in Rows)
                yield return new GridElement<TRow, TData> { Header = row, Value = Get(row, column) };
        }
    }
}

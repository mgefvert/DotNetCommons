using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetCommons.IO.Parsers
{
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

    internal class CsvFieldDefinition
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

    public enum CsvParserInvalidDataReason
    {
        RequiredValueMissing,
        ConversionFailed
    }

    public class CsvParserInvalidDataArgs : EventArgs
    {
        public Exception Exception { get; }
        public int? LineNo { get; }
        public CsvParserInvalidDataReason Reason { get; }

        public CsvParserInvalidDataArgs(int? lineNo, CsvParserInvalidDataReason reason, Exception exception)
        {
            LineNo = lineNo;
            Reason = reason;
            Exception = exception;
        }
    }

    public class CsvParser<T> where T : class, new()
    {
        public delegate void CsvParserInvalidDataDelegate(object sender, CsvParserInvalidDataArgs args);

        private readonly List<CsvFieldDefinition> _definitions = new List<CsvFieldDefinition>();
        private bool _gotHeaders;
        public CsvParser Parser { get; } = new CsvParser();
        public event CsvParserInvalidDataDelegate InvalidData;

        public CsvParser()
        {
            _definitions.AddRange(ProcessClass(typeof(T)));
        }

        protected void FireInvalidData(int? lineNo, CsvParserInvalidDataReason reason, Exception exception)
        {
            InvalidData?.Invoke(this, new CsvParserInvalidDataArgs(lineNo, reason, exception));
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

            foreach (var definition in _definitions)
            {
                var name = definition.Attribute.Name.ToLower();
                int value;
                if (lookup.TryGetValue(name, out value))
                    definition.FieldNo = value;
            }

            var missing = _definitions.Where(x => x.Attribute.Required && x.FieldNo == -1).ToList();
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

            var obj = new T();
            foreach (var definition in _definitions)
            {
                if (definition.FieldNo == -1)
                    continue;

                var value = strings.ElementAtOrDefault(definition.FieldNo);
                if (definition.Attribute.Required && string.IsNullOrWhiteSpace(value))
                {
                    FireInvalidData(lineNo, CsvParserInvalidDataReason.RequiredValueMissing, null);
                    continue;
                }

                try
                {
                    obj.SetPropertyValue(definition.Property, value);
                }
                catch (Exception ex)
                {
                    FireInvalidData(lineNo, CsvParserInvalidDataReason.ConversionFailed, ex);
                }
            }

            return obj;
        }
    }
}

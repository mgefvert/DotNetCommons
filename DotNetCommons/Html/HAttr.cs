using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DotNetCommons.Collections;

namespace DotNetCommons.Html
{
    public class HAttr : HElement
    {
        public string Key { get; set; }
        public SortedSet<string> Values { get; } = new SortedSet<string>();

        public HAttr()
        {
        }

        public HAttr(string key, params string[] values)
        {
            Key = key.ToLower();
            Values.AddRangeIfNotNull(values);
        }

        public void Add(string value)
        {
            Values.AddIfNotNull(value);
        }

        public void Add(params string[] values)
        {
            Values.AddRangeIfNotNull(values);
        }

        public void Clear()
        {
            Values.Clear();
        }

        public string GetString()
        {
            return string.Join(" ", Values);
        }

        public override string Render()
        {
            var result = WebUtility.HtmlEncode(Key);
            if (Values.Any())
                result += "=\"" + string.Join(" ", Values.Select(WebUtility.HtmlEncode)) + '"';

            return result;
        }

        public void Set(string value)
        {
            Values.Clear();
            Add(value);
        }

        public void Set(params string[] values)
        {
            Values.Clear();
            Add(values);
        }

        public override string ToString()
        {
            return Render();
        }
    }
}

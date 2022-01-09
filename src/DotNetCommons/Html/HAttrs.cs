using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Html;

public class HAttrs : HElement, IEnumerable<HAttr>
{
    private readonly Dictionary<string, HAttr> _attrs = new();

    public HAttrs()
    {
    }

    public HAttrs(params HAttr[] attributes)
    {
        foreach (var attr in attributes)
            Add(attr);
    }

    public static HAttrs Merge(params HAttrs[] attributelists)
    {
        var result = new HAttrs();
        if (attributelists != null)
            foreach (var attributes in attributelists)
            foreach (var attribute in attributes)
                result.Add(attribute);

        return result;
    }

    public HAttrs Add(HAttr attribute)
    {
        return Add(attribute.Key, attribute.Values.ToArray());
    }

    public HAttrs Add(string key, params string[] values)
    {
        var attr = _attrs.GetValueOrDefault(key);
        if (attr == null)
        {
            attr = new HAttr(key);
            _attrs[key] = attr;
        }

        attr.Add(values);
        return this;
    }

    public HAttr Get(string key)
    {
        return _attrs.GetValueOrDefault(key);
    }

    public string GetString(string key)
    {
        return _attrs.ContainsKey(key) ? _attrs[key].GetString() : null;
    }

    public IEnumerator<HAttr> GetEnumerator()
    {
        return _attrs.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string Render()
    {
        return string.Join(" ", _attrs.Values.OrderBy(x => x.Key).Select(x => x.Render()));
    }

    public HAttrs Set(HAttr attribute)
    {
        _attrs.Remove(attribute.Key);
        Add(attribute);
        return this;
    }

    public HAttrs Set(string key)
    {
        return Set(new HAttr(key));
    }

    public HAttrs Set(string key, string value)
    {
        return Set(new HAttr(key, value));
    }

    public HAttrs Set(string key, params string[] values)
    {
        return Set(new HAttr(key, values));
    }

    public override string ToString()
    {
        return Render();
    }
}
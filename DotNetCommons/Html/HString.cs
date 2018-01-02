using System;
using System.Collections.Generic;
using System.Net;

namespace DotNetCommons.Html
{
    public class HString : HElement, IEqualityComparer<HString>
    {
        private string _html;

        protected HString()
        {
        }

        public static HString Raw(string html)
        {
            return new HString { _html = html };
        }

        public static HString Encode(string html)
        {
            return new HString { _html = WebUtility.HtmlEncode(html) };
        }

        public HString Copy()
        {
            return new HString { _html = _html };
        }

        public override string Render()
        {
            return _html;
        }

        public override string ToString()
        {
            return _html;
        }

        public static implicit operator HString(string text)
        {
            return Encode(text);
        }

        public static HString operator +(HString h1, HString h2)
        {
            return new HString { _html = h1._html + h2._html };
        }

        public bool Equals(HString x, HString y)
        {
            return string.Equals(x._html, y._html);
        }

        public int GetHashCode(HString obj)
        {
            return obj._html.GetHashCode();
        }
    }
}

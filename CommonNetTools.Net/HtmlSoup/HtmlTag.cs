using System;
using System.Collections.Generic;

namespace CommonNetTools.Net.HtmlSoup
{
    public class HtmlTag : HtmlElement
    {
        public string Tag { get; set; }
        public bool Closing { get; set; }
        public Dictionary<string, string> Attributes { get; private set; }

        public HtmlTag()
        {
            Attributes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public HtmlTag(string text) : this()
        {
            text = (text ?? "").Trim().TrimStart('<').TrimEnd('>').Trim();
            if (text == "")
                return;

            if (text.StartsWith("/"))
            {
                Closing = true;
                text = text.TrimStart('/').Trim();
            }

            Tag = ExtractString(ref text).ToLower();

            while (!string.IsNullOrEmpty(text))
            {
                var key = ExtractString(ref text).ToLower();
                if (string.IsNullOrEmpty(key))
                    break;

                if (Peek(text) == '=')
                {
                    text = text.TrimStart('=').Trim();
                    var value = ExtractString(ref text);
                    Attributes[key] = value;
                }
                else
                    Attributes[key] = "";
            }
        }

        public string GetAttribute(string attribute)
        {
            string value;
            return Attributes.TryGetValue(attribute, out value) ? value : null;
        }

        private static string ExtractString(ref string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return null;

            // Determine what type we're looking for here
            var c = tag[0];

            if (c == '"' || c == '\'')
                return ExtractQuotedString(ref tag, c);

            // Extract word
            var i = tag[0] == '/' ? 1 : 0;
            while (i < tag.Length && (char.IsLetterOrDigit(tag[i]) || tag[i] == '-' || tag[i] == '.'))
                i++;

            var result = tag.Substring(0, i);
            tag = tag.Substring(i).Trim();

            return result;
        }

        private static string ExtractQuotedString(ref string tag, char c)
        {
            var i = 1;
            while (i < tag.Length && tag[i] != c)
            {
                if (c == '\\')
                    i++;
                i++;
            }

            var result = tag.Substring(1, i - 1);
            tag = tag.Substring(i + 1).Trim();

            return result;
        }

        private static char Peek(string tag)
        {
            return string.IsNullOrEmpty(tag) ? '\0' : tag[0];
        }
    }
}

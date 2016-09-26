using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonNetTools.Net.HtmlSoup
{
    public static class HtmlParser
    {
        public static List<HtmlElement> Parse(string html)
        {
            var result = new List<HtmlElement>();

            var sb = new StringBuilder();
            var tag = false;
            foreach (var c in html)
            {
                if (tag)
                {
                    sb.Append(c);

                    if (c == '>')
                    {
                        AddElementToResult(sb, true, result);
                        tag = false;
                    }
                }
                else
                {
                    if (c == '<')
                    {
                        AddElementToResult(sb, false, result);
                        tag = true;
                    }

                    sb.Append(c);
                }
            }

            AddElementToResult(sb, tag, result);

            return result;
        }

        private static void AddElementToResult(StringBuilder sb, bool tag, List<HtmlElement> result)
        {
            var str = sb.ToString().Trim();
            if (string.IsNullOrEmpty(str))
                return;

            if (tag)
                result.Add(new HtmlTag(sb.ToString()));
            else
                result.Add(new HtmlText(sb.ToString()));

            sb.Clear();
        }

        private static bool IsTag(HtmlElement element, string name)
        {
            var tag = element as HtmlTag;
            if (tag == null)
                return false;

            if (string.IsNullOrEmpty(name))
                return false;

            return name.Equals(tag.Tag, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsAttribute(HtmlElement element, string attribute, string value)
        {
            var tag = element as HtmlTag;
            if (tag == null)
                return false;

            if (string.IsNullOrEmpty(attribute) || value == null)
                return false;

            return tag.Attributes.ContainsKey(attribute) && value.Equals(tag.Attributes[attribute], StringComparison.CurrentCultureIgnoreCase);
        }

        public static IEnumerable<HtmlTag> FindTag(this IEnumerable<HtmlElement> soup, string tag)
        {
            return soup.OfType<HtmlTag>().Where(x => IsTag(x, tag));
        }

        public static IEnumerable<HtmlTag> FindTag(this IEnumerable<HtmlElement> soup, string tag, string attribute, string value)
        {
            return soup.OfType<HtmlTag>().Where(x => IsTag(x, tag) && IsAttribute(x, attribute, value));
        }

        public static IEnumerable<HtmlTagContainer> FindContainer(this IEnumerable<HtmlElement> soup, string tag, string attribute = null, string value = null)
        {
            HtmlTagContainer result = null;
            var level = 0;

            foreach (var x in soup)
            {
                var xtag = x as HtmlTag;

                // If we have an HtmlTag and it matches the tag we're looking for
                if (xtag != null && IsTag(xtag, tag))
                {
                    // Increase or decrease the limiter
                    if (xtag.Closing)
                    {
                        level = Math.Max(0, level - 1);
                        if (level == 0 && result != null)
                        {
                            // End of search group
                            yield return result;
                            result = null;
                        }
                    }
                    else
                    {
                        // If we're already running a search, increase level
                        if (result != null)
                            level++;

                        // Else figure out if we're matching attributes as well (if necessary) - if so, start search
                        else if (attribute == null || value == null || IsAttribute(xtag, attribute, value))
                        {
                            level++;
                            result = new HtmlTagContainer(xtag);
                            continue;
                        }
                    }
                }

                // If we have an existing search, just keep adding
                if (result != null)
                    result.Children.Add(x);
            }

            if (result != null)
                yield return result;
        }

        public static Dictionary<string, string> ParseForm(this IEnumerable<HtmlElement> soup)
        {
            var result = new Dictionary<string, string>();
            foreach (var tag in soup.FindTag("input"))
            {
                var key = tag.GetAttribute("name");
                if (string.IsNullOrEmpty(key))
                    continue;

                result[key] = tag.GetAttribute("value");
            }

            return result;
        }
    }
}

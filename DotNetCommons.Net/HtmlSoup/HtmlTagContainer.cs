using System;
using System.Collections.Generic;

namespace DotNetCommons.Net.HtmlSoup
{
    public class HtmlTagContainer : HtmlTag
    {
        public List<HtmlElement> Children { get; } = new List<HtmlElement>();

        public HtmlTagContainer()
        {
        }

        public HtmlTagContainer(HtmlTag source) : this()
        {
            Tag = source.Tag;
            Closing = source.Closing;
            foreach (var item in source.Attributes)
                Attributes[item.Key] = item.Value;
        }
    }
}

using System;
using System.Collections.Generic;

namespace DotNetCommons.MicroWeb.HtmlSoup
{
    public class HtmlTagContainer : HtmlTag
    {
        public List<HtmlElement> Children { get; private set; }

        public HtmlTagContainer()
        {
            Children = new List<HtmlElement>();
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

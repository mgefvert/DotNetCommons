using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Html
{
    public class HTag : HElement
    {
        public HAttrs Attributes { get; } = new HAttrs();
        public string Name { get; set; }
        public List<HElement> Children { get; } = new List<HElement>();

        public HTag()
        {
        }

        public HTag(string name)
        {
            Name = name;
        }

        public HTag(string name, params HElement[] elements) : this(name)
        {
            Add(elements);
        }

        public HTag Add(IEnumerable<HElement> elements)
        {
            if (elements != null)
                foreach (var element in elements)
                    Add(element);

            return this;
        }

        public HTag Add(params HElement[] elements)
        {
            if (elements != null)
                foreach (var element in elements)
                    Add(element);

            return this;
        }

        public HTag Add(HElement element)
        {
            if (element is HAttr attr)
                Attributes.Add(attr);
            else if (element is HAttrs attrs)
            {
                foreach (var x in attrs)
                    Attributes.Add(x);
            }
            else if (element != null)
                Children.Add(element);

            return this;
        }

        public override string Render()
        {
            if (!Children.Any())
                return RenderEmptyTag();

            var sb = new StringBuilder();
            sb.Append(RenderOpenTag());

            foreach (var child in Children)
                sb.Append(child.Render());

            sb.Append(RenderCloseTag());
            return sb.ToString();
        }

        public string RenderOpenTag()
        {
            return $"<{RenderTagAndAttributes()}>";
        }

        public string RenderCloseTag()
        {
            return "</" + Name + ">";
        }

        public string RenderEmptyTag()
        {
            return $"<{RenderTagAndAttributes()}/>";
        }

        public string RenderTagAndAttributes()
        {
            var result = Name;
            if (Attributes.Any())
                result += " " + Attributes.Render();

            return result;
        }
    }
}

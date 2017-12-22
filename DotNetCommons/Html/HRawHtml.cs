using System;

namespace DotNetCommons.Html
{
    public class HRawHtml : HElement
    {
        public string Html { get; set; }

        public HRawHtml()
        {
        }

        public HRawHtml(string html)
        {
            Html = html;
        }

        public override string Render()
        {
            return Html;
        }
    }
}

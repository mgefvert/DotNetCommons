using System;

namespace CommonNetTools.Scripting.HtmlSoup
{
    public class HtmlText : HtmlElement
    {
        public string Text { get; set; }

        public HtmlText()
        {
        }

        public HtmlText(string text) : this()
        {
            Text = (text ?? "").Trim();
        }
    }
}

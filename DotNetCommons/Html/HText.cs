using System;
using System.Net;

namespace DotNetCommons.Html
{
    public class HText : HElement
    {
        public string Text { get; set; }

        public HText()
        {
        }

        public HText(string content)
        {
            Text = WebUtility.HtmlEncode(content);
        }

        public override string Render()
        {
            return Text;
        }
    }
}

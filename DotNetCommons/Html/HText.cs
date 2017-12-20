using System;
using System.Web;

namespace DotNetCommons.Html
{
    public class HText : HElement
    {
        public HtmlString Content { get; set; }

        public HText()
        {
        }

        public HText(string content)
        {
            Content = new HtmlString(HttpUtility.HtmlEncode(content));
        }

        public HText(HtmlString content)
        {
            Content = content;
        }

        public override string Render()
        {
            return Content.ToString();
        }
    }
}

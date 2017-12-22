using System;
using System.Web;
using DotNetCommons.Html;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Html
{
    [TestClass]
    public class HTagTest
    {
        [TestMethod]
        public void TestNoAttr()
        {
            var tag = new HTag("div", new HText("I'm a text!"), new HRawHtml("<br>"));

            Assert.AreEqual("<div>I&#39;m a text!<br></div>", tag.Render());
            Assert.AreEqual("</div>", tag.RenderCloseTag());
            Assert.AreEqual("<div/>", tag.RenderEmptyTag());
            Assert.AreEqual("<div>", tag.RenderOpenTag());
            Assert.AreEqual("div", tag.RenderTagAndAttributes());
        }

        [TestMethod]
        public void Test1Attr()
        {
            var tag = new HTag("div", new HAttr("class", "btn"), 
                new HText("I'm a text!"), new HRawHtml("<br>"));

            Assert.AreEqual("<div class=\"btn\">I&#39;m a text!<br></div>", tag.Render());
            Assert.AreEqual("</div>", tag.RenderCloseTag());
            Assert.AreEqual("<div class=\"btn\"/>", tag.RenderEmptyTag());
            Assert.AreEqual("<div class=\"btn\">", tag.RenderOpenTag());
            Assert.AreEqual("div class=\"btn\"", tag.RenderTagAndAttributes());
        }

        [TestMethod]
        public void Test2Attr()
        {
            var tag = new HTag("div", new HAttrs(new HAttr("class", "btn"), new HAttr("name", "control")),
                new HText("I'm a text!"), 
                new HRawHtml("<br>"));

            Assert.AreEqual("<div class=\"btn\" name=\"control\">I&#39;m a text!<br></div>", tag.Render());
            Assert.AreEqual("</div>", tag.RenderCloseTag());
            Assert.AreEqual("<div class=\"btn\" name=\"control\"/>", tag.RenderEmptyTag());
            Assert.AreEqual("<div class=\"btn\" name=\"control\">", tag.RenderOpenTag());
            Assert.AreEqual("div class=\"btn\" name=\"control\"", tag.RenderTagAndAttributes());
        }
    }
}

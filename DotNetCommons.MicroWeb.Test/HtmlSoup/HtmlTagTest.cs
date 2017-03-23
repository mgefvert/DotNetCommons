using System;
using DotNetCommons.MicroWeb.HtmlSoup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.MicroWeb.Test.HtmlSoup
{
    [TestClass]
    public class HtmlTagTest
    {
        [TestMethod]
        public void ParseSimpleTag()
        {
            var tag = new HtmlTag("<img>");
            Assert.AreEqual("img", tag.Tag);
            Assert.AreEqual(false, tag.Closing);
        }

        [TestMethod]
        public void ParseSimpleClosingTag()
        {
            var tag = new HtmlTag("</img>");
            Assert.AreEqual("img", tag.Tag);
            Assert.AreEqual(true, tag.Closing);
        }

        [TestMethod]
        public void ParseAttributeSingle()
        {
            var tag = new HtmlTag("<form name=\"x\">");
            Assert.AreEqual("form", tag.Tag);
            Assert.AreEqual(false, tag.Closing);
            Assert.AreEqual("x", tag.GetAttribute("name"));
        }

        [TestMethod]
        public void ParseAttributeMultiple()
        {
            var tag = new HtmlTag("<form disabled name=\"x\" id='form'>");
            Assert.AreEqual("form", tag.Tag);
            Assert.AreEqual(false, tag.Closing);
            Assert.AreEqual("", tag.GetAttribute("disabled"));
            Assert.AreEqual("x", tag.GetAttribute("name"));
            Assert.AreEqual("form", tag.GetAttribute("id"));
        }

        [TestMethod]
        public void ParseComplexForm()
        {
            var tag = new HtmlTag("<form accept-charset=\"UTF-8\" action=\"/sign-in\" method=\"post\">");
            Assert.AreEqual("form", tag.Tag);
            Assert.AreEqual(false, tag.Closing);
            Assert.AreEqual("UTF-8", tag.GetAttribute("accept-charset"));
            Assert.AreEqual("/sign-in", tag.GetAttribute("action"));
            Assert.AreEqual("post", tag.GetAttribute("method"));
        }
    }
}

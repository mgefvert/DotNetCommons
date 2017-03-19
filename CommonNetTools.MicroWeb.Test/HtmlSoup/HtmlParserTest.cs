using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetTools.Scripting.HtmlSoup;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonNetTools.MicroWeb.Test.HtmlSoup
{
    [TestClass]
    public class HtmlParserTest
    {
        private List<HtmlElement> _soup;

        [TestInitialize]
        public void Setup()
        {
            _soup = HtmlParser.Parse("<html><body><form name='my'><input name='x' value='42'><input name='y' value='none'><div>This is some text!<br>And more!</div></form></body></html>");
        }

        [TestMethod]
        public void Parse()
        {
            Assert.AreEqual(11, _soup.OfType<HtmlTag>().Count());
            Assert.AreEqual(2, _soup.OfType<HtmlText>().Count());
            Assert.AreEqual("html", ((HtmlTag)_soup[0]).Tag);
        }

        [TestMethod]
        public void FindTag()
        {
            var inputs = _soup.FindTag("input").ToList();

            Assert.AreEqual(2, inputs.Count);
            Assert.AreEqual("x", inputs[0].GetAttribute("name"));
            Assert.AreEqual("y", inputs[1].GetAttribute("name"));
        }

        [TestMethod]
        public void FindTagByName()
        {
            var inputs = _soup.FindTag("input", "name", "x").ToList();

            Assert.AreEqual(1, inputs.Count);
            Assert.AreEqual("x", inputs[0].GetAttribute("name"));
        }

        [TestMethod]
        public void FindContainer()
        {
            var form = _soup.FindContainer("form").Single();

            Assert.AreEqual("form", form.Tag);
            Assert.AreEqual("my", form.GetAttribute("name"));
            Assert.AreEqual(7, form.Children.Count);
        }

        [TestMethod]
        public void FindMultipleLevelContainer()
        {
            _soup = HtmlParser.Parse("<html>0<div>1<div id='target'>2<div>3</div>4</div>5</div>6</html>");

            var found = _soup.FindContainer("div", "id", "target").ToList();
            var text = string.Join("", found.Single().Children.OfType<HtmlText>().Select(x => x.Text));

            Assert.AreEqual("234", text);
        }

        [TestMethod]
        public void FindContainerByName()
        {
            var form = _soup.FindContainer("form", "name", "my").Single();

            Assert.AreEqual("form", form.Tag);
            Assert.AreEqual("my", form.GetAttribute("name"));
            Assert.AreEqual(7, form.Children.Count);
        }

        [TestMethod]
        public void ParseForm()
        {
            var form = _soup.FindContainer("form").Single().Children.ParseForm();

            Assert.AreEqual(2, form.Count);
            Assert.AreEqual("42", form["x"]);
            Assert.AreEqual("none", form["y"]);
        }
    }
}

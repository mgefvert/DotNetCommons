using System;
using DotNetCommons.Html;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Html
{
    [TestClass]
    public class HAttrTest
    {
        [TestMethod]
        public void Test()
        {
            var attr = new HAttr("class", "btn", "btn-primary");
            Assert.AreEqual("class=\"btn btn-primary\"", attr.Render());

            attr.Add("test");
            Assert.AreEqual("class=\"btn btn-primary test\"", attr.Render());

            attr.Set("test");
            Assert.AreEqual("class=\"test\"", attr.Render());
        }
    }
}

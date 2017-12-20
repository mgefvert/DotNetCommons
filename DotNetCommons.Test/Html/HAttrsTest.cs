using System;
using DotNetCommons.Html;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Html
{
    [TestClass]
    public class HAttrsTest
    {
        [TestMethod]
        public void Test()
        {
            var attrs = new HAttrs
            {
                { "class", "btn", "btn-primary" },
                { "name", "control" }
            };

            Assert.AreEqual("btn btn-primary", attrs.GetString("class"));
            Assert.AreEqual("control", attrs.GetString("name"));

            var attr = new HAttr("test", "123");
            attrs.Add(attr);
            attrs.Get("test").Set("456");

            Assert.AreEqual("123", attr.GetString());
            Assert.AreEqual("456", attrs.GetString("test"));

            attrs.Add("class", "test");
            Assert.AreEqual("class=\"btn btn-primary test\" name=\"control\" test=\"456\"", attrs.Render());
        }

        [TestMethod]
        public void TestMerge()
        {
            var attrs1 = new HAttrs(new HAttr("class", "btn"));
            var attrs2 = new HAttrs(new HAttr("class", "btn-primary"), new HAttr("name", "control"));
            var result = HAttrs.Merge(attrs1, attrs2);

            Assert.AreEqual("class=\"btn\"", attrs1.Render());
            Assert.AreEqual("class=\"btn-primary\" name=\"control\"", attrs2.Render());
            Assert.AreEqual("class=\"btn btn-primary\" name=\"control\"", result.Render());
        }
    }
}

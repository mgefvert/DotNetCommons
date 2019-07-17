using System;
using DotNetCommons.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.IoC
{
    public class SubclassedContainer : MicroContainer
    {
        public IBar Bar => Acquire<IBar>();

        public string Email
        {
            get => GetConfigValue<string>("email");
            set => SetConfig("email", value);
        }

        public int Port
        {
            get => GetConfigValue<int>("port");
            set => SetConfig("port", value);
        }
    }

    [TestClass]
    public class SubclassedContainerTest
    {
        [TestMethod]
        public void Test()
        {
            var sub = new SubclassedContainer();
            sub
                .Register<IFoo, Foo>(CreationMode.Create)
                .Register<IBar, Bar>(CreationMode.Singleton)
                .SetConfig("email", "joe")
                .SetConfig("port", 42);

            Assert.AreEqual("Hello from Foo", sub.Bar.Foo.Message);
            Assert.AreEqual("joe", sub.Email);
            Assert.AreEqual(42, sub.Port);
        }
    }
}

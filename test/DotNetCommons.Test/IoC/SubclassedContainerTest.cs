using System;
using DotNetCommons.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.IoC
{
    public class SubclassedContainer : MicroContainer
    {
        public IBar Bar => Create<IBar>();

        public string Email
        {
            get => Get<string>("email");
            set => Set("email", value);
        }

        public int Port
        {
            get => Get<int>("port");
            set => Set("port", value);
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
                .Register<IFoo, Foo>()
                .Register<IBar, Bar>(CreationMode.Singleton)
                .Set("email", "joe")
                .Set("port", 42);

            Assert.AreEqual("Hello from Foo", sub.Bar.Foo.Message);
            Assert.AreEqual("joe", sub.Email);
            Assert.AreEqual(42, sub.Port);
        }
    }
}

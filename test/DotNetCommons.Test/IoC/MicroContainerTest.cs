using System;
using DotNetCommons.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.IoC
{
    public interface IFoo
    {
        string Message { get; }
    }

    public class Foo : IFoo
    {
        string IFoo.Message => "Hello from Foo";
    }

    public interface IBar
    {
        IFoo Foo { get; }
    }

    public class Bar : IBar
    {
        public IFoo Foo { get; }

        public Bar(IFoo foo)
        {
            Foo = foo;
        }
    }

    public interface IFooBar
    {
        IFoo Foo { get; }
        IBar Bar { get; }
        int Value { get; }
    }

    public class FooBar : IFooBar
    {
        public IFoo Foo { get; set; }
        public IBar Bar { get; set; }
        public int Value { get; set; }

        public FooBar()
        {
        }

        public FooBar(IFoo foo, int value = 0)
        {
            Foo = foo;
            Value = value;
        }
    }

    public interface IPersistent
    {
    }

    public class Persistent : IPersistent
    {
    }

    [TestClass]
    public class MicroContainerTest
    {
        private MicroContainer _container;

        [TestInitialize]
        public void Setup()
        {
            _container = new MicroContainer()
                .Register<IFoo, Foo>(CreationMode.Create)
                .Register<IBar, Bar>(CreationMode.Singleton)
                .Register<IFooBar, FooBar>(CreationMode.Create);
        }

        [TestMethod]
        public void CreateFoo()
        {
            var foo = _container.Acquire<IFoo>();
            Assert.IsInstanceOfType(foo, typeof(Foo));
            Assert.AreEqual("Hello from Foo", foo.Message);
        }

        [TestMethod]
        public void CreateBar()
        {
            var bar = _container.Acquire<IBar>();
            Assert.IsInstanceOfType(bar, typeof(Bar));
            Assert.IsInstanceOfType(bar.Foo, typeof(Foo));
            Assert.AreEqual("Hello from Foo", bar.Foo.Message);

            var bar2 = _container.Acquire<IBar>();
            Assert.IsTrue(bar == bar2);
        }

        [TestMethod]
        public void CreateFooBar()
        {
            var foobar = _container.Acquire<IFooBar>();
            Assert.IsInstanceOfType(foobar, typeof(FooBar));
            Assert.IsInstanceOfType(foobar.Foo, typeof(Foo));
            Assert.IsInstanceOfType(foobar.Bar, typeof(Bar));
            Assert.AreEqual("Hello from Foo", foobar.Foo.Message);
        }

        [TestMethod]
        public void TestSingletonSpeed()
        {
            GC.Collect();

            var t0 = DateTime.Now;
            for (int i = 0; i < 500_000; i++)
                _container.Acquire<IBar>();
            Console.WriteLine("Singleton completed in " + (DateTime.Now - t0).TotalMilliseconds + " msec");

            GC.Collect();

            t0 = DateTime.Now;
            for (int i = 0; i < 500_000; i++)
                _container.Acquire<IFoo>();
            Console.WriteLine("Creation completed in " + (DateTime.Now - t0).TotalMilliseconds + " msec");
        }

        [TestMethod]
        public void Local()
        {
            var original = new Persistent();
            _container.Register<IPersistent>(original);

            var bar = _container.Acquire<IBar>();
            var persist = _container.Acquire<IPersistent>();
            Assert.IsNotNull(bar);
            Assert.IsTrue(persist == original);

            var local = _container.Local();
            var bar2 = local.Acquire<IBar>();
            var persist2 = local.Acquire<IPersistent>();
            Assert.IsFalse(bar2 == bar);
            Assert.IsTrue(persist2 == persist);
        }
    }
}

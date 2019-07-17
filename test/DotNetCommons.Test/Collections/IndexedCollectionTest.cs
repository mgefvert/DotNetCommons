using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Collections
{
    public class Foo
    {
        public string Name { get; }
        public int Age { get; }

        public Foo(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }

    [TestClass]
    public class IndexedCollectionTest
    {
        private string Fetch<TItem, TResult>(IEnumerable<TItem> items, Func<TItem, TResult> accessor)
        {
            return string.Join(",", items.Select(accessor).OrderBy(x => x));
        }

        [TestMethod]
        public void Test()
        {
            var list = new IndexedCollection<Foo>();
            list.DefineIndex("Name", x => x.Name);
            list.DefineIndex("Age", x => x.Age);

            list.AddRange(new[] {
                new Foo("John", 42),
                new Foo("John", 40),
                new Foo("Sandy", 42),
                new Foo("Eric", 19),
                new Foo("Mike", 31)
            });

            Assert.AreEqual("", Fetch(list.Lookup("Name", "None"), x => x.Age));
            Assert.AreEqual("31", Fetch(list.Lookup("Name", "Mike"), x => x.Age));
            Assert.AreEqual("40,42", Fetch(list.Lookup("Name", "John"), x => x.Age));

            Assert.AreEqual("", Fetch(list.Lookup("Name", "None"), x => x.Age));
            Assert.AreEqual("31", Fetch(list.Lookup("Name", "Mike"), x => x.Age));
            Assert.AreEqual("40,42", Fetch(list.Lookup("Name", "John"), x => x.Age));

            Assert.AreEqual("", Fetch(list.Lookup("Age", 0), x => x.Name));
            Assert.AreEqual("Mike", Fetch(list.Lookup("Age", 31), x => x.Name));
            Assert.AreEqual("John,Sandy", Fetch(list.Lookup("Age", 42), x => x.Name));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test
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

    public class FooList : IndexedList<Foo>
    {
        public FooList()
        {
            DefineIndex("Name", x => x.Name);
            DefineIndex("Age", x => x.Age);
        }

        public IndexedListItem<Foo> Name => FindIndex("Name");
        public IndexedListItem<Foo> Age => FindIndex("Age");
    }

    [TestClass]
    public class IndexedListTest
    {
        private string Fetch<TItem, TResult>(IEnumerable<TItem> items, Func<TItem, TResult> accessor)
        {
            return string.Join(",", items.Select(accessor).OrderBy(x => x));
        }

        [TestMethod]
        public void Test()
        {
            var list = new FooList
            {
                new Foo("John", 42),
                new Foo("John", 40),
                new Foo("Sandy", 42),
                new Foo("Eric", 19),
                new Foo("Mike", 31)
            };

            Assert.AreEqual("", Fetch(list.Lookup("Name", "None"), x => x.Age));
            Assert.AreEqual("31", Fetch(list.Lookup("Name", "Mike"), x => x.Age));
            Assert.AreEqual("40,42", Fetch(list.Lookup("Name", "John"), x => x.Age));

            Assert.AreEqual("", Fetch(list.Name["None"], x => x.Age));
            Assert.AreEqual("31", Fetch(list.Name["Mike"], x => x.Age));
            Assert.AreEqual("40,42", Fetch(list.Name["John"], x => x.Age));

            Assert.AreEqual("", Fetch(list.Age[0], x => x.Name));
            Assert.AreEqual("Mike", Fetch(list.Age[31], x => x.Name));
            Assert.AreEqual("John,Sandy", Fetch(list.Age[42], x => x.Name));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonNetTools.IO.Test
{
    [TestClass]
    public class KeyValueStoreTest
    {
        [TestMethod]
        public void TestSaveAndLoad()
        {
            var dict = new Dictionary<int, string>
            {
                [1] = "Adam",
                [2] = "Sandy",
                [-3] = "Bertha"
            };

            var store = new KeyValueStore();
            var memory = new MemoryStream();

            store.Save(dict, memory);
            Assert.IsTrue(memory.Position > 10);

            memory.Position = 0;
            var result = store.Load<int, string>(memory);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Adam", result[1]);
            Assert.AreEqual("Sandy", result[2]);
            Assert.AreEqual("Bertha", result[-3]);
        }

        [TestMethod]
        public void TestSpeed()
        {
            var dict = new Dictionary<int, string>();

            for (var i = 1; i <= 100000; i++)
                dict[i] = "This is string number " + i;

            var store = new KeyValueStore();
            var memory = new MemoryStream();

            var t0 = DateTime.Now;

            store.Save(dict, memory);
            memory.Position = 0;
            var result = store.Load<int, string>(memory);

            Console.WriteLine((DateTime.Now - t0).TotalMilliseconds + " ms");

            Assert.AreEqual(100000, result.Count);
            Assert.AreEqual("This is string number 4711", result[4711]);
        }
    }
}

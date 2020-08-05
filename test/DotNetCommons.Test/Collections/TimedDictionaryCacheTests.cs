using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ODataService.Classes;

namespace ODataService.Tests.Classes
{
    [TestClass]
    public class TimedDictionaryCacheTests
    {
        private class TestObject
        {
            public string Value { get; }

            public TestObject(string value)
            {
                Value = value;
            }
        }

        private TimedDictionaryCache<string, TestObject> _cache;

        [TestInitialize]
        public void Setup()
        {
            _cache = new TimedDictionaryCache<string, TestObject>(TimeSpan.FromSeconds(1));
        }

        private async Task Populate()
        {
            await _cache.Set("answer", new TestObject("42"));
            await _cache.Set("foo", new TestObject("bar"));
            await _cache.Set("random", new TestObject("4711"));
            await _cache.Set("randomer", new TestObject("11147"));
        }

        [TestMethod]
        public async Task StressTest()
        {
            var random = new Random();
            var cache = new TimedDictionaryCache<string, TestObject>(TimeSpan.FromSeconds(1))
            {
                LoadObject = async s =>
                {
                    await Task.Delay(300);
                    return new TestObject(s);
                }
            };

            var now = DateTime.Now;
            var numbers = Enumerable.Range(1, 500).ToList();

            var tasks = numbers
                .Select(x => Task.Run(() =>
                {
                    var n = random.Next(1, 30).ToString();
                    return new Tuple<string, Task<TestObject>>(n, cache.Get(n));
                }))
                .ToArray();
            await Task.WhenAll(tasks);

            foreach (var task in tasks)
                Assert.AreEqual(task.Result.Item1, task.Result.Item2.Result.Value);

            Console.WriteLine($"Stress test finished in {(DateTime.Now - now).TotalMilliseconds} ms");
        }

        [TestMethod]
        public async Task TestCount()
        {
            Assert.AreEqual(0, _cache.Count());
            await Populate();
            Assert.AreEqual(4, _cache.Count());
        }

        [TestMethod]
        public async Task Clear()
        {
            await Populate();
            Assert.AreEqual(4, _cache.Count());
            _cache.Clear();
            Assert.AreEqual(0, _cache.Count());
        }

        [TestMethod]
        public async Task TestExists()
        {
            await Populate();
            Assert.IsTrue(_cache.Exists("foo"));
            Assert.IsFalse(_cache.Exists("nonexisting"));
        }

        [TestMethod]
        public async Task TestPurge()
        {
            await Populate();
            Assert.AreEqual(4, _cache.Count());

            await Task.Delay(1200);

            Assert.AreEqual(0, _cache.Count());
        }

        [TestMethod]
        public async Task TestSet()
        {
            Assert.AreEqual(null, (await _cache.Get("foo"))?.Value);
            await Populate();
            Assert.AreEqual("bar", (await _cache.Get("foo"))?.Value);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task TestSet_OnlyNonNull()
        {
            await _cache.Set("foo", null);
        }

        [TestMethod]
        public async Task TestLoadConcurrency()
        {
            var i = 1;
            _cache.LoadObject = async s =>
            {
                await Task.Delay(300);
                return new TestObject((i++).ToString());
            };

            var numbers = Enumerable.Range(1, 4).ToList();

            var firstTasks = numbers.Select(x => Task.Run(() => _cache.Get("test"))).ToArray();
            await Task.WhenAll(firstTasks);
            var first = firstTasks.Select(x => x.Result).ToList();

            await Task.Delay(1200);

            var secondTasks = numbers.Select(x => Task.Run(() => _cache.Get("test"))).ToArray();
            await Task.WhenAll(secondTasks);
            var second = secondTasks.Select(x => x.Result).ToList();

            Assert.AreEqual(4, first.Count);
            Assert.AreEqual("1", first[0].Value);
            Assert.AreEqual("1", first[1].Value);
            Assert.AreEqual("1", first[2].Value);
            Assert.AreEqual("1", first[3].Value);

            Assert.AreEqual(4, second.Count);
            Assert.AreEqual("2", second[0].Value);
            Assert.AreEqual("2", second[1].Value);
            Assert.AreEqual("2", second[2].Value);
            Assert.AreEqual("2", second[3].Value);
        }
    }
}

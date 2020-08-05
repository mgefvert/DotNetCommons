using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ODataService.Classes;

namespace ODataService.Tests.Classes
{
    [TestClass]
    public class TimedListCacheTests
    {
        private TimedListCache<int> _cache;

        [TestInitialize]
        public void Setup()
        {
            _cache = new TimedListCache<int>(TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public async Task StressTest()
        {
            _cache.LoadObject = async () =>
            {
                await Task.Delay(100);
                return new List<int> { 1 };
            };

            var now = DateTime.Now;
            var numbers = Enumerable.Range(1, 500).ToList();

            var tasks = numbers.Select(x => Task.Run(() => _cache.Get())).ToArray();
            await Task.WhenAll(tasks);

            var result = new List<int> { 1 };
            foreach (var task in tasks)
                CollectionAssert.AreEqual(result, task.Result);

            Console.WriteLine($"Stress test finished in {(DateTime.Now - now).TotalMilliseconds} ms");
        }

        [TestMethod]
        public async Task TestCount()
        {
            Assert.AreEqual(0, await _cache.Count());
            await _cache.Set(new List<int> { 42, 4711, 11147 });
            Assert.AreEqual(3, await _cache.Count());
        }

        [TestMethod]
        public async Task Clear()
        {
            await _cache.Set(new List<int> { 42, 4711, 11147 });
            Assert.AreEqual(3, await _cache.Count());
            await _cache.Clear();
            Assert.AreEqual(0, await _cache.Count());
        }

        [TestMethod]
        public async Task TestExists()
        {
            Assert.IsFalse(await _cache.Exists());
            await _cache.Set(new List<int> { 42 });
            Assert.IsTrue(await _cache.Exists());
        }

        [TestMethod]
        public async Task TestPurge()
        {
            await _cache.Set(new List<int> { 42 });
            Assert.AreEqual(1, await _cache.Count());

            await Task.Delay(1200);

            Assert.AreEqual(0, await _cache.Count());
        }

        [TestMethod]
        public async Task TestSet()
        {
            Assert.AreEqual(null, await _cache.Get());
            
            await _cache.Set(new List<int> { 42 });
            CollectionAssert.AreEqual(new List<int> { 42 }, await _cache.Get());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task TestSet_OnlyNonNull()
        {
            await _cache.Set(null);
        }

        [TestMethod]
        public async Task TestLoadConcurrency()
        {
            var i = 1;

            var cache = new TimedListCache<int>(TimeSpan.FromSeconds(1))
            {
                LoadObject = async () =>
                {
                    await Task.Delay(300);
                    return new List<int> {i++};
                }
            };

            var numbers = Enumerable.Range(1, 4).ToList();

            var firstTasks = numbers.Select(x => Task.Run(() => cache.Get())).ToArray();
            await Task.WhenAll(firstTasks);
            var first = firstTasks.Select(x => x.Result).ToList();

            await Task.Delay(1200);

            var secondTasks = numbers.Select(x => Task.Run(() => cache.Get())).ToArray();
            await Task.WhenAll(secondTasks);
            var second = secondTasks.Select(x => x.Result).ToList();

            Assert.AreEqual(4, first.Count);
            Assert.AreEqual(1, first[0].Single());
            Assert.AreEqual(1, first[1].Single());
            Assert.AreEqual(1, first[2].Single());
            Assert.AreEqual(1, first[3].Single());

            Assert.AreEqual(4, second.Count);
            Assert.AreEqual(2, second[0].Single());
            Assert.AreEqual(2, second[1].Single());
            Assert.AreEqual(2, second[2].Single());
            Assert.AreEqual(2, second[3].Single());
        }
    }
}

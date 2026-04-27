using DotNetCommons.Collections;

namespace DotNetCommonTests.Collections;

[TestClass]
public class ObjectCacheTests
{
    private sealed record TestItem(int Id, string Name);
    private sealed record OtherItem(int Id);
    private sealed record StressItem(int WriterId, int Iteration, string Key);

    [TestMethod]
    public void Get_ReturnsNull_WhenTypeHasNotBeenCached()
    {
        var cache = new ObjectCache();

        var result = cache.Get<TestItem>();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Cache_StoresItemsByType()
    {
        var cache = new ObjectCache();
        var items = new[]
        {
            new TestItem(1, "one"),
            new TestItem(2, "two")
        };

        cache.Cache(items);

        CollectionAssert.AreEqual(items, cache.Get<TestItem>());
        Assert.IsNull(cache.Get<OtherItem>());
    }

    [TestMethod]
    public void Cache_StoresSingleItemAsArray()
    {
        var cache = new ObjectCache();
        var item = new TestItem(1, "one");

        cache.Cache(item);

        CollectionAssert.AreEqual(new[] { item }, cache.Get<TestItem>());
    }

    [TestMethod]
    public void Cache_ReplacesExistingBucketForType()
    {
        var cache = new ObjectCache();
        var first = new[] { new TestItem(1, "one") };
        var second = new[] { new TestItem(2, "two"), new TestItem(3, "three") };

        cache.Cache(first);
        cache.Cache(second);

        CollectionAssert.AreEqual(second, cache.Get<TestItem>());
    }

    [TestMethod]
    public void KeyedCache_KeepsSameTypeBucketsSeparate()
    {
        var cache = new ObjectCache();
        var defaultItems = new[] { new TestItem(1, "default") };
        var firstKeyItems = new[] { new TestItem(2, "first") };
        var secondKeyItems = new[] { new TestItem(3, "second") };

        cache.Cache(defaultItems);
        cache.Cache("first", firstKeyItems);
        cache.Cache("second", secondKeyItems);

        CollectionAssert.AreEqual(defaultItems, cache.Get<TestItem>());
        CollectionAssert.AreEqual(firstKeyItems, cache.Get<TestItem>("first"));
        CollectionAssert.AreEqual(secondKeyItems, cache.Get<TestItem>("second"));
    }

    [TestMethod]
    public void Invalidate_RemovesOnlyDefaultBucketForType()
    {
        var cache = new ObjectCache();
        var keyedItems = new[] { new TestItem(2, "keyed") };

        cache.Cache([new TestItem(1, "default")]);
        cache.Cache("keyed", keyedItems);
        cache.Cache([new OtherItem(3)]);

        cache.Invalidate<TestItem>();

        Assert.IsNull(cache.Get<TestItem>());
        CollectionAssert.AreEqual(keyedItems, cache.Get<TestItem>("keyed"));
        Assert.IsNotNull(cache.Get<OtherItem>());
    }

    [TestMethod]
    public void Invalidate_WithKey_RemovesOnlySpecificKeyedBucket()
    {
        var cache = new ObjectCache();
        var firstKeyItems = new[] { new TestItem(1, "first") };
        var otherTypeItems = new[] { new OtherItem(2) };

        cache.Cache("first", firstKeyItems);
        cache.Cache("second", [new TestItem(3, "second")]);
        cache.Cache("second", otherTypeItems);

        cache.Invalidate<TestItem>("second");

        CollectionAssert.AreEqual(firstKeyItems, cache.Get<TestItem>("first"));
        Assert.IsNull(cache.Get<TestItem>("second"));
        CollectionAssert.AreEqual(otherTypeItems, cache.Get<OtherItem>("second"));
    }

    [TestMethod]
    public void InvalidateAllOfType_RemovesDefaultAndKeyedBucketsForType()
    {
        var cache = new ObjectCache();
        var otherTypeItems = new[] { new OtherItem(3) };

        cache.Cache([new TestItem(1, "default")]);
        cache.Cache("keyed", [new TestItem(2, "keyed")]);
        cache.Cache("keyed", otherTypeItems);

        cache.InvalidateAllOfType<TestItem>();

        Assert.IsNull(cache.Get<TestItem>());
        Assert.IsNull(cache.Get<TestItem>("keyed"));
        CollectionAssert.AreEqual(otherTypeItems, cache.Get<OtherItem>("keyed"));
    }

    [TestMethod]
    public void InvalidateAll_RemovesEveryBucket()
    {
        var cache = new ObjectCache();

        cache.Cache([new TestItem(1, "default")]);
        cache.Cache("keyed", [new OtherItem(2)]);

        cache.InvalidateAll();

        Assert.IsNull(cache.Get<TestItem>());
        Assert.IsNull(cache.Get<OtherItem>("keyed"));
    }

    [TestMethod]
    public void Get_ExpiresBucketAfterTimeout()
    {
        var cache = new ObjectCache(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(1));

        cache.Cache([new TestItem(1, "expired")]);

        Thread.Sleep(80);

        Assert.IsNull(cache.Get<TestItem>());
    }

    [TestMethod]
    public void Cache_ThrowsWhenTimeoutsAreInvalid()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ObjectCache(TimeSpan.Zero, TimeSpan.FromSeconds(1)));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ObjectCache(TimeSpan.FromSeconds(1), TimeSpan.Zero));
    }

    [TestMethod]
    public void KeyedCache_ThrowsWhenKeyIsNull()
    {
        var cache = new ObjectCache();

        Assert.ThrowsExactly<ArgumentNullException>(() => cache.Get<TestItem>(null!));
    }

    [TestMethod]
    [Timeout(15000)]
    public async Task StressTest_MultipleReadersAndWriters_AreThreadSafe()
    {
        const int writerCount = 8;
        const int readerCount = 16;
        const int keysPerWriter = 16;
        const int iterations = 5000;

        var cache = new ObjectCache(TimeSpan.FromMinutes(10), TimeSpan.FromMilliseconds(1));
        var start = new ManualResetEventSlim(false);
        var failures = new List<Exception>();
        var failureLock = new object();

        void RecordFailure(Exception exception)
        {
            lock (failureLock)
                failures.Add(exception);
        }

        var writers = Enumerable.Range(0, writerCount)
            .Select(writerId => Task.Run(() =>
            {
                start.Wait();

                try
                {
                    for (var iteration = 0; iteration < iterations; iteration++)
                    {
                        var key = "key-" + (iteration % keysPerWriter);
                        var item = new StressItem(writerId, iteration, key);

                        cache.Cache(key, [item, item]);

                        if ((iteration & 255) == 0)
                            cache.Cache([new OtherItem(iteration)]);
                    }
                }
                catch (Exception exception)
                {
                    RecordFailure(exception);
                }
            }))
            .ToArray();

        var readers = Enumerable.Range(0, readerCount)
            .Select(readerId => Task.Run(() =>
            {
                start.Wait();

                try
                {
                    for (var iteration = 0; iteration < iterations; iteration++)
                    {
                        var key = "key-" + ((readerId + iteration) % keysPerWriter);
                        var items = cache.Get<StressItem>(key);

                        if (items is null)
                            continue;

                        Assert.HasCount(2, items);
                        Assert.IsTrue(items.All(item => item.Key == key));
                        Assert.AreEqual(items[0], items[1]);

                        _ = cache.Get<OtherItem>();
                    }
                }
                catch (Exception exception)
                {
                    RecordFailure(exception);
                }
            }))
            .ToArray();

        var invalidator = Task.Run(() =>
        {
            start.Wait();

            try
            {
                for (var iteration = 0; iteration < iterations / 10; iteration++)
                {
                    cache.Invalidate<StressItem>("missing-" + iteration);

                    if ((iteration & 31) == 0)
                        cache.Invalidate<OtherItem>();
                }
            }
            catch (Exception exception)
            {
                RecordFailure(exception);
            }
        });

        start.Set();
        await Task.WhenAll(writers.Concat(readers).Append(invalidator));

        if (failures.Count > 0)
            Assert.Fail(string.Join(Environment.NewLine, failures.Select(failure => failure.ToString())));
    }
}

using DotNetCommons.Synchronization;
using FluentAssertions;

namespace DotNetCommonTests.Synchronization;

[TestClass]
public class AccessCacheTests
{
    public class MyObject
    {
        public int Value { get; set; }
    }

    public class OtherObject
    {
        public int Value { get; set; }
    }

    private AccessCache _accessCache = null!;

    [TestInitialize]
    public void Initialize()
    {
        _accessCache = new AccessCache(TimeSpan.FromSeconds(5));
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_WhenCacheIsEmpty_WaitsForSingleFactoryCall()
    {
        var releaseFactory = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var factoryCalls = 0;

        async Task<MyObject> Factory()
        {
            Interlocked.Increment(ref factoryCalls);
            await releaseFactory.Task;
            return new MyObject { Value = 1 };
        }

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _accessCache.GetOrReplaceAsync<MyObject>(Factory))
            .ToArray();

        await Task.Delay(50);
        factoryCalls.Should().Be(1);

        releaseFactory.SetResult();
        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(result => ReferenceEquals(result, results[0]));
        results[0].Value.Should().Be(1);
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_WhenCacheHasNotExpired_ReturnsCachedValue()
    {
        var factoryCalls = 0;

        Task<MyObject> Factory()
        {
            var value = Interlocked.Increment(ref factoryCalls);
            return Task.FromResult(new MyObject { Value = value });
        }

        var first = await _accessCache.GetOrReplaceAsync<MyObject>(Factory);
        var second = await _accessCache.GetOrReplaceAsync<MyObject>(Factory);

        second.Should().BeSameAs(first);
        factoryCalls.Should().Be(1);
    }

    [TestMethod]
    public async Task Expire_WhenCacheHasValue_WaitsForReplacementValue()
    {
        var first = await _accessCache.GetOrReplaceAsync(() => Task.FromResult(new MyObject { Value = 1 }));
        var refreshStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseRefresh = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _accessCache.Expire<MyObject>();

        var secondTask = _accessCache.GetOrReplaceAsync<MyObject>(async () =>
        {
            refreshStarted.SetResult();
            await releaseRefresh.Task;
            return new MyObject { Value = 2 };
        });

        await refreshStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        secondTask.IsCompleted.Should().BeFalse();

        releaseRefresh.SetResult();
        var second = await secondTask;

        second.Value.Should().Be(2);
        second.Should().NotBeSameAs(first);
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_WhenWaitingForFirstValue_CanBeCancelled()
    {
        var releaseFactory = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cts = new CancellationTokenSource();

        var waitTask = _accessCache.GetOrReplaceAsync<MyObject>(
            async () =>
            {
                await releaseFactory.Task;
                return new MyObject { Value = 1 };
            },
            cts.Token);

        await Task.Delay(50);
        await cts.CancelAsync();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await waitTask);

        releaseFactory.SetResult();
        var value = await _accessCache.GetOrReplaceAsync(() => Task.FromResult(new MyObject { Value = 2 }));

        value.Value.Should().Be(1);
    }

    [TestMethod]
    public async Task Clear_WhenCacheHasValues_RemovesAllCachedValues()
    {
        var myObjectCalls = 0;
        var otherObjectCalls = 0;

        var firstMyObject = await _accessCache.GetOrReplaceAsync(() =>
            Task.FromResult(new MyObject { Value = Interlocked.Increment(ref myObjectCalls) }));
        var firstOtherObject = await _accessCache.GetOrReplaceAsync(() =>
            Task.FromResult(new OtherObject { Value = Interlocked.Increment(ref otherObjectCalls) }));

        _accessCache.Clear();

        var secondMyObject = await _accessCache.GetOrReplaceAsync(() =>
            Task.FromResult(new MyObject { Value = Interlocked.Increment(ref myObjectCalls) }));
        var secondOtherObject = await _accessCache.GetOrReplaceAsync(() =>
            Task.FromResult(new OtherObject { Value = Interlocked.Increment(ref otherObjectCalls) }));

        secondMyObject.Should().NotBeSameAs(firstMyObject);
        secondMyObject.Value.Should().Be(2);
        secondOtherObject.Should().NotBeSameAs(firstOtherObject);
        secondOtherObject.Value.Should().Be(2);
    }

    [TestMethod]
    public async Task Clear_WhenInitialRefreshIsRunning_RemovesPendingEntryFromCache()
    {
        var firstFactoryStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstFactory = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var firstTask = _accessCache.GetOrReplaceAsync<MyObject>(async () =>
        {
            firstFactoryStarted.SetResult();
            await releaseFirstFactory.Task;
            return new MyObject { Value = 1 };
        });

        await firstFactoryStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));

        _accessCache.Clear();

        var second = await _accessCache.GetOrReplaceAsync(() => Task.FromResult(new MyObject { Value = 2 }));

        releaseFirstFactory.SetResult();
        var first = await firstTask;
        var third = await _accessCache.GetOrReplaceAsync(() => Task.FromResult(new MyObject { Value = 3 }));

        first.Value.Should().Be(1);
        second.Value.Should().Be(2);
        third.Should().BeSameAs(second);
    }

    [TestMethod]
    public async Task ExpireAll_WhenCacheHasValues_ReplacesEveryCachedValue()
    {
        var firstMyObject = await _accessCache.GetOrReplaceAsync(() => Task.FromResult(new MyObject { Value = 1 }));
        var firstOtherObject = await _accessCache.GetOrReplaceAsync(() => Task.FromResult(new OtherObject { Value = 1 }));

        _accessCache.ExpireAll();

        var secondMyObject = await _accessCache.GetOrReplaceAsync(() => Task.FromResult(new MyObject { Value = 2 }));
        var secondOtherObject = await _accessCache.GetOrReplaceAsync(() => Task.FromResult(new OtherObject { Value = 2 }));

        secondMyObject.Should().NotBeSameAs(firstMyObject);
        secondMyObject.Value.Should().Be(2);
        secondOtherObject.Should().NotBeSameAs(firstOtherObject);
        secondOtherObject.Value.Should().Be(2);
    }
}

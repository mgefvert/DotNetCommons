using DotNetCommons.Synchronization;
using FluentAssertions;

namespace DotNetCommonTests.Synchronization;

[TestClass]
public class AccessCacheTests
{
    [TestMethod]
    public async Task GetOrReplaceAsync_WithoutCachedValue_CreatesValue()
    {
        var cache = new AccessCache();
        var value = new CachedValue(1);

        var result = await cache.GetOrReplaceAsync(() => Task.FromResult(value));

        result.Should().BeSameAs(value);
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_WithCachedValue_ReturnsCachedValue()
    {
        var cache        = new AccessCache(TimeSpan.FromMinutes(1));
        var cachedValue  = new CachedValue(1);
        var ignoredValue = new CachedValue(2);
        var calls        = 0;

        var first = await cache.GetOrReplaceAsync(() =>
        {
            calls++;
            return Task.FromResult(cachedValue);
        });

        var second = await cache.GetOrReplaceAsync(() =>
        {
            calls++;
            return Task.FromResult(ignoredValue);
        });

        first.Should().BeSameAs(cachedValue);
        second.Should().BeSameAs(cachedValue);
        calls.Should().Be(1);
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_WhenExpired_ReplacesBeforeReturning()
    {
        var cache = new AccessCache(TimeSpan.Zero);
        var first = new CachedValue(1);
        var second = new CachedValue(2);

        var firstResult  = await cache.GetOrReplaceAsync(() => Task.FromResult(first));
        var secondResult = await cache.GetOrReplaceAsync(() => Task.FromResult(second));

        firstResult.Should().BeSameAs(first);
        secondResult.Should().BeSameAs(second);
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_CachesValuesByType()
    {
        var cache      = new AccessCache(TimeSpan.FromMinutes(1));
        var firstValue = new CachedValue(1);
        var otherValue = new OtherCachedValue(2);

        var firstResult = await cache.GetOrReplaceAsync(() => Task.FromResult(firstValue));
        var otherResult = await cache.GetOrReplaceAsync(() => Task.FromResult(otherValue));

        firstResult.Should().BeSameAs(firstValue);
        otherResult.Should().BeSameAs(otherValue);
    }

    [TestMethod]
    public async Task Clear_RemovesOnlySpecifiedType()
    {
        var cache       = new AccessCache(TimeSpan.FromMinutes(1));
        var firstValue  = new CachedValue(1);
        var secondValue = new CachedValue(2);
        var otherValue  = new OtherCachedValue(3);
        var replacement = new OtherCachedValue(4);

        await cache.GetOrReplaceAsync(() => Task.FromResult(firstValue));
        await cache.GetOrReplaceAsync(() => Task.FromResult(otherValue));

        cache.Clear<CachedValue>();

        var firstResult = await cache.GetOrReplaceAsync(() => Task.FromResult(secondValue));
        var otherResult = await cache.GetOrReplaceAsync(() => Task.FromResult(replacement));

        firstResult.Should().BeSameAs(secondValue);
        otherResult.Should().BeSameAs(otherValue);
    }

    [TestMethod]
    public async Task ClearAll_RemovesAllCachedTypes()
    {
        var cache       = new AccessCache(TimeSpan.FromMinutes(1));
        var firstValue  = new CachedValue(1);
        var secondValue = new CachedValue(2);
        var otherValue  = new OtherCachedValue(3);
        var replacement = new OtherCachedValue(4);

        await cache.GetOrReplaceAsync(() => Task.FromResult(firstValue));
        await cache.GetOrReplaceAsync(() => Task.FromResult(otherValue));

        cache.ClearAll();

        var firstResult = await cache.GetOrReplaceAsync(() => Task.FromResult(secondValue));
        var otherResult = await cache.GetOrReplaceAsync(() => Task.FromResult(replacement));

        firstResult.Should().BeSameAs(secondValue);
        otherResult.Should().BeSameAs(replacement);
    }

    [TestMethod]
    public async Task Expire_ExpiresOnlySpecifiedType()
    {
        var cache       = new AccessCache(TimeSpan.FromMinutes(1));
        var firstValue  = new CachedValue(1);
        var secondValue = new CachedValue(2);
        var otherValue  = new OtherCachedValue(3);
        var replacement = new OtherCachedValue(4);

        await cache.GetOrReplaceAsync(() => Task.FromResult(firstValue));
        await cache.GetOrReplaceAsync(() => Task.FromResult(otherValue));

        cache.Expire<CachedValue>();

        var firstResult = await cache.GetOrReplaceAsync(() => Task.FromResult(secondValue));
        var otherResult = await cache.GetOrReplaceAsync(() => Task.FromResult(replacement));

        firstResult.Should().BeSameAs(secondValue);
        otherResult.Should().BeSameAs(otherValue);
    }

    [TestMethod]
    public async Task ExpireAll_ExpiresAllCachedTypes()
    {
        var cache       = new AccessCache(TimeSpan.FromMinutes(1));
        var firstValue  = new CachedValue(1);
        var secondValue = new CachedValue(2);
        var otherValue  = new OtherCachedValue(3);
        var replacement = new OtherCachedValue(4);

        await cache.GetOrReplaceAsync(() => Task.FromResult(firstValue));
        await cache.GetOrReplaceAsync(() => Task.FromResult(otherValue));

        cache.ExpireAll();

        var firstResult = await cache.GetOrReplaceAsync(() => Task.FromResult(secondValue));
        var otherResult = await cache.GetOrReplaceAsync(() => Task.FromResult(replacement));

        firstResult.Should().BeSameAs(secondValue);
        otherResult.Should().BeSameAs(replacement);
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_WhenFactoryThrows_DoesNotCacheFailure()
    {
        var cache = new AccessCache();
        var value = new CachedValue(1);
        var calls = 0;

        Func<Task> act = async () => await cache.GetOrReplaceAsync<CachedValue>(() =>
        {
            calls++;
            throw new InvalidOperationException("Factory failed.");
        });

        await act.Should().ThrowAsync<InvalidOperationException>();

        var result = await cache.GetOrReplaceAsync(() =>
        {
            calls++;
            return Task.FromResult(value);
        });

        result.Should().BeSameAs(value);
        calls.Should().Be(2);
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_WhenFactoryIsNull_Throws()
    {
        var cache = new AccessCache();

        Func<Task> act = async () => await cache.GetOrReplaceAsync<CachedValue>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_WhenWaitingForInitialValueAndTokenCancels_Throws()
    {
        var cache          = new AccessCache();
        var createdValue   = new CachedValue(1);
        var factoryStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFactory = new TaskCompletionSource<CachedValue>(TaskCreationOptions.RunContinuationsAsynchronously);

        var firstTask = cache.GetOrReplaceAsync(() =>
        {
            factoryStarted.SetResult();
            return releaseFactory.Task;
        });
        await factoryStarted.Task;

        using var cts = new CancellationTokenSource();
        var secondTask = cache.GetOrReplaceAsync(() => Task.FromResult(new CachedValue(2)), cts.Token);

        cts.Cancel();

        await secondTask.Invoking(async task => await task).Should().ThrowAsync<TaskCanceledException>();

        releaseFactory.SetResult(createdValue);
        (await firstTask).Should().BeSameAs(createdValue);
    }

    [TestMethod]
    public async Task GetOrReplaceAsync_ConcurrentInitialRequests_RunSingleFactory()
    {
        var cache          = new AccessCache(TimeSpan.FromMinutes(1));
        var value          = new CachedValue(1);
        var calls          = 0;
        var factoryStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFactory = new TaskCompletionSource<CachedValue>(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => cache.GetOrReplaceAsync(() =>
            {
                Interlocked.Increment(ref calls);
                factoryStarted.SetResult();
                return releaseFactory.Task;
            }))
            .ToList();

        await factoryStarted.Task;
        releaseFactory.SetResult(value);

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(result => ReferenceEquals(result, value));
        calls.Should().Be(1);
    }

    private sealed record CachedValue(int Id);

    private sealed record OtherCachedValue(int Id);
}

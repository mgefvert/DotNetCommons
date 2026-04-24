using DotNetCommons.Synchronization;

namespace DotNetCommonTests.Synchronization;

[TestClass]
public class AccessorTests
{
    private Accessor<object> _accessor = null!;

    [TestInitialize]
    public void Initialize()
    {
        _accessor = new Accessor<object>();
    }

    [TestMethod]
    public void ConstructorWithValue_SetsValue()
    {
        var value    = new object();
        var accessor = new Accessor<object>(value);

        Assert.IsTrue(accessor.Exists);
        Assert.AreSame(value, accessor.TryGet());
    }

    [TestMethod]
    public void Clear_RemovesValue()
    {
        var value = new object();
        _accessor.Replace(value);
        _accessor.Clear();

        Assert.IsFalse(_accessor.Exists);
        Assert.IsNull(_accessor.TryGet());
    }

    [TestMethod]
    public async Task Clear_ResetsGetAsync()
    {
        var value1 = new object();
        var value2 = new object();

        _accessor.Replace(value1);
        _accessor.Clear();

        var task = _accessor.GetAsync();
        Assert.IsFalse(task.IsCompleted);

        _accessor.Replace(value2);
        var result = await task;

        Assert.AreSame(value2, result);
    }

    [TestMethod]
    public void Exists_InitiallyFalse()
    {
        Assert.IsFalse(_accessor.Exists);
        Assert.IsNull(_accessor.TryGet());
    }

    [TestMethod]
    public async Task GetAsync_CancellationToken_AbortsWait()
    {
        using var cts  = new CancellationTokenSource();
        var       task = _accessor.GetAsync(cts.Token);

        Assert.IsFalse(task.IsCompleted);

        cts.Cancel();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await task);
    }

    [TestMethod]
    public async Task GetAsync_Cancelled_Throws()
    {
        using var cts = new CancellationTokenSource();

        cts.Cancel();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await _accessor.GetAsync(cts.Token));
    }

    [TestMethod]
    public async Task GetAsync_Concurrency_MultipleWaitersAndReplacers()
    {
        const int waiterCount   = 50;
        const int replacerCount = 50;
        var       value         = new object();

        var waiters = Enumerable.Range(0, waiterCount)
            .Select(_ => Task.Run(() => _accessor.GetAsync()))
            .ToList();

        var replacers = Enumerable.Range(0, replacerCount)
            .Select(_ => Task.Run(() => _accessor.Replace(value)))
            .ToList();

        await Task.WhenAll(replacers);
        var results = await Task.WhenAll(waiters);

        foreach (var result in results)
        {
            Assert.AreSame(value, result);
        }
    }

    [TestMethod]
    public async Task GetAsync_MultipleWaiters_AllGetSameValue()
    {
        var tasks = Enumerable.Range(0, 10).Select(_ => _accessor.GetAsync()).ToList();

        var value = new object();
        _accessor.Replace(value);

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
            Assert.AreSame(value, result);
    }

    [TestMethod]
    public async Task GetAsync_WaitAfterClear_ReturnsNewValue()
    {
        var task1 = _accessor.GetAsync();
        _accessor.Clear();

        var value1 = new object();
        _accessor.Replace(value1);

        // task1 is likely orphaned because it's waiting on the old TCS.
        // Let's verify this behavior.

        var delayTask     = Task.Delay(100);
        var completedTask = await Task.WhenAny(task1, delayTask);

        Assert.AreSame(delayTask, completedTask, "task1 should NOT have completed because it was waiting on a pre-clear TCS");

        var task2   = _accessor.GetAsync();
        var result2 = await task2;
        Assert.AreSame(value1, result2, "task2 should have completed with value1");
    }

    [TestMethod]
    public async Task GetAsync_WithValue_ReturnsImmediately()
    {
        var value = new object();
        _accessor.Replace(value);

        var result = await _accessor.GetAsync();

        Assert.AreSame(value, result);
    }

    [TestMethod]
    public async Task GetAsync_WithoutValue_WaitsForReplace()
    {
        var value = new object();
        var task  = _accessor.GetAsync();

        Assert.IsFalse(task.IsCompleted);

        _accessor.Replace(value);

        var result = await task;
        Assert.AreSame(value, result);
    }

    [TestMethod]
    public void Replace_SetsValueAndExists()
    {
        var value = new object();
        _accessor.Replace(value);

        Assert.IsTrue(_accessor.Exists);
        Assert.AreSame(value, _accessor.TryGet());
    }
}

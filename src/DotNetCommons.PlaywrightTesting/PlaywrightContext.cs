using Microsoft.Playwright;

namespace DotNetCommons.PlaywrightTesting;

public class PlaywrightContext : IAsyncDisposable
{
    private readonly PlaywrightSession _session;
    private readonly Uri _root;

    public IBrowserContext Context { get; }

    internal PlaywrightContext(PlaywrightSession session, Uri root, IBrowserContext context)
    {
        _session = session;
        _root = root;
        Context = context;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.CloseAsync();
    }

    public async Task<PlaywrightPage> NewPage(string name)
    {
        var screenShotHelper = _session.ScreenShotDirectory != null
            ? new ScreenShotHelper(_session.ScreenShotDirectory, name)
            : null;

        return new PlaywrightPage(await Context.NewPageAsync(), _root, name, screenShotHelper);
    }

    public async Task RunInNewPage(string name, Func<PlaywrightPage, Task> action)
    {
        await using var page = await NewPage(name);
        await action(page);
    }
    
    public Task RunInParallel(params Func<Task>[] tasks)
    {
        return RunInParallel(3, tasks);
    }

    public async Task RunInParallel(int parallelism, params Func<Task>[] tasks)
    {
        var source = tasks.ToList();
        var running = new List<Task>();

        while (source.Any() || running.Any())
        {
            while (running.Count < parallelism && source.Any())
            {
                var func = source.ExtractFirst();
                var task = func();
                running.Add(task);
            }

            await Task.WhenAny(running);
            running.ExtractAll(t => t.Status is TaskStatus.Canceled or TaskStatus.Faulted or TaskStatus.RanToCompletion);
        }
    }
}
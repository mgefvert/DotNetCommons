using System.Diagnostics;
using System.Reflection;
using DotNetCommons.Sys;
using Microsoft.Playwright;

namespace DotNetCommons.PlaywrightTesting;

public class PlaywrightSession : IAsyncDisposable
{
    public DirectoryInfo? ScreenShotDirectory { get; }
    public IPlaywright? Session { get; private set; }
    public IBrowser? Browser { get; private set; } 

    public PlaywrightSession(string? screenShotDirectory = null)
    {
        if (screenShotDirectory != null)
        {
            ScreenShotDirectory = new DirectoryInfo(screenShotDirectory);
            if (ScreenShotDirectory.Exists)
                ScreenShotDirectory.Delete(true);
            ScreenShotDirectory?.Create();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser != null)
        {
            await Browser.CloseAsync();
            Browser = null;
        }

        if (Session != null)
        {
            Session.Dispose();
            Session = null;
        }
    }

    public async Task<PlaywrightContext> NewContext(Uri root)
    {
        await Start();
        return new PlaywrightContext(this, root, await Browser!.NewContextAsync());
    }

    private async Task Start()
    {
        Session ??= await Playwright.CreateAsync();
        Browser ??= await Session.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !Debugger.IsAttached
        });
    }

    public Task RunAllTests(Uri root) => RunAllTests(root, Assembly.GetCallingAssembly());

    public async Task RunAllTests(Uri root, Assembly assembly)
    {
        Console.WriteLine("Starting test run...");
        
        var runner = new TestRunner(this, root, assembly);
        await runner.Run();
        Console.WriteLine();
        var tests = runner.Results.Count;
        var fails = runner.Results.Where(x => !x.Success).ToList();

        if (tests == 0)
        {
            using (new SetConsoleColor(ConsoleColor.Yellow))
                Console.WriteLine("No tests found.");
        }
        else if (!fails.Any())
        {
            using (new SetConsoleColor(ConsoleColor.Green))
                Console.WriteLine($"{tests} tests succeeded.");
        }
        else
        {
            Console.WriteLine();
            using (new SetConsoleColor(ConsoleColor.Red))
            {
                Console.WriteLine($"{fails.Count} out of {tests} tests failed:");
                foreach (var fail in fails)
                    Console.WriteLine($"{fail.ClassName}.{fail.MethodName}: {fail.Message}");
            }
        }
    }
}
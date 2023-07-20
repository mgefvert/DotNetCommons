using Microsoft.Playwright;

// ReSharper disable MemberCanBePrivate.Global

namespace DotNetCommons.PlaywrightTesting;

public class PlaywrightPage : IAsyncDisposable
{
    private static readonly SemaphoreSlim ScreenShotLock = new SemaphoreSlim(1, 1);
    private readonly ScreenShotHelper? _screenShots;
    private readonly Uri _root;
    private readonly string _name;

    public IPage Page { get; }
    
    internal PlaywrightPage(IPage page, Uri root, string name, ScreenShotHelper? screenShots)
    {
        _root = root;
        _name = name;
        _screenShots = screenShots;
        
        Page = page;
        Page.RequestFinished += (_, request) =>
        {
            if (request.Failure.IsSet())
                throw new PlaywrightTestingException($"Request failed ({request.Failure}): {request.Url}");
        };
        Page.RequestFailed += (_, request) => throw new PlaywrightTestingException($"Request failed ({request.Failure}): {request.Url}");
        Page.PageError += (_, s) => throw new PlaywrightTestingException($"Error occurred on page: {s}");
        
        Console.WriteLine($"-> Creating new page: {_name}");
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine($"<- Disposing page: {_name}");
        await Page.CloseAsync();
    }

    public async Task<bool> IsActive(string selector) => await IsVisible(selector) && await IsEnabled(selector);
    public async Task<bool> IsChecked(string selector) => await Page.IsCheckedAsync(selector);
    public async Task<bool> IsDisabled(string selector) => await Page.IsDisabledAsync(selector);
    public async Task<bool> IsEditable(string selector) => await Page.IsEditableAsync(selector);
    public async Task<bool> IsEnabled(string selector) => await Page.IsEnabledAsync(selector);
    public async Task<bool> IsHidden(string selector) => await Page.IsHiddenAsync(selector);
    public async Task<bool> IsVisible(string selector) => await Page.IsVisibleAsync(selector);

    public Task<bool> IsUrl(string url)
    {
        var uri = new Uri(Page.Url);
        return Task.FromResult(uri.AbsolutePath.Contains(url, StringComparison.CurrentCultureIgnoreCase));
    }
    
    public async Task Verify(Func<Task<bool>> condition, string failMessage)
    {
        var waiting = false;
        var timeout = DateTime.UtcNow.AddSeconds(3);
        while (DateTime.UtcNow < timeout)
        {
            var result = await condition();
            if (result)
            {
                if (waiting)
                    Console.WriteLine($"{_name}: Wait OK");
                return;
            }

            if (!waiting)
            {
                waiting = true;
                Console.WriteLine($"{_name}: Waiting...");
            }

            await Task.Delay(100);
        }

        Console.WriteLine($"{_name}: Wait failed: {failMessage}");
        throw new PlaywrightTestingException(failMessage);
    }

    public Task VerifyActive(string selector) => Verify(async () => await IsActive(selector), $"'{selector}' is not visible and enabled");
    public Task VerifyChecked(string selector) => Verify(async () => await IsChecked(selector), $"'{selector}' is not checked");
    public Task VerifyDisabled(string selector) => Verify(async () => await IsDisabled(selector), $"'{selector}' is not disabled");
    public Task VerifyEditable(string selector) => Verify(async () => await IsEditable(selector), $"'{selector}' is not editable");
    public Task VerifyEnabled(string selector) => Verify(async () => await IsEnabled(selector), $"'{selector}' is not enabled");
    public Task VerifyHidden(string selector) => Verify(async () => await IsHidden(selector), $"'{selector}' is not hidden");
    public Task VerifyVisible(string selector) => Verify(async () => await IsVisible(selector), $"'{selector}' is not visible");

    public Task VerifyContainsText(string text) => Verify(async () => await ContainsText(text), $"Page does not contain '{text}'");
    public Task VerifyIsUrl(string url) => Verify(async () => await IsUrl(url), $"Current url is not '{url}'");
    public Task VerifyNotContainsText(string text) => Verify(async () => !await ContainsText(text), $"Page contains '{text}'");

    public Task VerifyActive(string selector, string failMessage) => Verify(async () => await IsActive(selector), failMessage);
    public Task VerifyChecked(string selector, string failMessage) => Verify(async () => await IsChecked(selector), failMessage);
    public Task VerifyDisabled(string selector, string failMessage) => Verify(async () => await IsDisabled(selector), failMessage);
    public Task VerifyEditable(string selector, string failMessage) => Verify(async () => await IsEditable(selector), failMessage);
    public Task VerifyEnabled(string selector, string failMessage) => Verify(async () => await IsEnabled(selector), failMessage);
    public Task VerifyHidden(string selector, string failMessage) => Verify(async () => await IsHidden(selector), failMessage);
    public Task VerifyVisible(string selector, string failMessage) => Verify(async () => await IsVisible(selector), failMessage);

    public Task VerifyContainsText(string text, string failMessage) => Verify(async () => await ContainsText(text), failMessage);
    public Task VerifyIsUrl(string url, string failMessage) => Verify(async () => await IsUrl(url), failMessage);
    public Task VerifyNotContainsText(string text, string failMessage) => Verify(async () => !await ContainsText(text), failMessage);
    
    public async Task<bool> Await(Func<Task<bool>> check, int? seconds = null)
    {
        var horizon = DateTime.UtcNow.AddSeconds(seconds ?? 5);

        while (DateTime.UtcNow < horizon)
        {
            if (await check())
                return true;
            
            await Task.Delay(50);
        }

        return false;
    }

    public async Task AwaitEnabled(string selector, int? seconds = null)
    {
        var result = await Await(async () => await IsEnabled(selector), seconds);
        if (!result)
            throw new PlaywrightTestingException($"Timeout while waiting for '{selector}' to become enabled");
    }

    public async Task Check(string selector, bool onOrOff)
    {
        Console.WriteLine($"{_name}: Check {selector}={onOrOff}");
        await Page.SetCheckedAsync(selector, onOrOff);
    }

    public async Task Click(string selector)
    {
        Console.WriteLine($"{_name}: Click {selector}");
        if (!await Page.IsVisibleAsync(selector))
            throw new PlaywrightTestingException($"Selector {selector} is not visible");

        await Page.ClickAsync(selector);
        await Page.WaitForLoadStateAsync(LoadState.Load);
    }

    public async Task<bool> ContainsText(string text)
    {
        var pageText = await Page.ContentAsync();
        return pageText.Contains(text, StringComparison.CurrentCultureIgnoreCase);
    }

    public async Task Focus(string selector)
    {
        Console.WriteLine($"{_name}: Focus {selector}");
        if (!await Page.IsVisibleAsync(selector))
            throw new PlaywrightTestingException($"Selector {selector} is not visible");

        await Page.FocusAsync(selector, new PageFocusOptions
        {
            Timeout = 1000
        });
    }

    public async Task Navigate(string path)
    {
        Console.WriteLine($"{_name}: Navigating to {path}");
        var uri = new Uri(_root, path);
        var response = await Page.GotoAsync(uri.ToString());
        if (response == null || !response.Ok)
            throw new PlaywrightTestingException($"Unable to navigate to {uri}");

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task Screenshot(string actionName)
    {
        if (_screenShots == null)
            return;

        await ScreenShotLock.WaitAsync();
        try
        {
            Console.WriteLine($"{_name}: Screenshot: {actionName}");
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = _screenShots.MakeFileName(actionName)
            });
        }
        finally
        {
            ScreenShotLock.Release();
        }
    }

    public async Task ScreenShotAfter(string sectionName, Func<Task> action)
    {
        try
        {
            await action();
        }
        finally
        {
            await Screenshot(sectionName);
        }
    }

    public async Task TypeIn(string selector, string text)
    {
        await Focus(selector);
        Console.WriteLine($"{_name}: TypeIn: '{text}'");
        await Page.Keyboard.TypeAsync(text);
    }
}
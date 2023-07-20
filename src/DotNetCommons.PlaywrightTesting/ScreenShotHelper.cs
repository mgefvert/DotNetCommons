using System.Text.RegularExpressions;

namespace DotNetCommons.PlaywrightTesting;

public class ScreenShotHelper
{
    private static readonly Regex WashRegex = new(@"[^a-zA-Z0-9]");
    private readonly DirectoryInfo _directory;
    private int _counter;

    public ScreenShotHelper(DirectoryInfo screenShotDirectory, string folder)
    {
        _directory = new DirectoryInfo(Path.Combine(screenShotDirectory.FullName, WashName(folder)));
        if (!_directory.Exists)
            _directory.Create();
    }

    private string WashName(string name)
    {
        var result = WashRegex.Replace(name, "-").Trim('-');
        while (result.Contains("--"))
            result = result.Replace("--", "-");

        return result;
    }

    public string MakeFileName(string actionName)
    {
        return Path.Combine(_directory.FullName, $"{++_counter:D3}-{WashName(actionName)}.png");
    }
}
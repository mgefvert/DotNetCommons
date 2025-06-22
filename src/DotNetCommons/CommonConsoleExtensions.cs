// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

/// <summary>
/// Helper class that checks to see if a console is attached to the current process without
/// throwing any exceptions.
/// </summary>
public static class ConsoleExtensions
{
    private static bool? _hasConsole;

    /// <summary>
    /// True if the current process has a console, false if headless.
    /// </summary>
    public static bool HasConsole => _hasConsole ?? (bool)(_hasConsole = CheckForConsole());

    private static bool CheckForConsole()
    {
        try
        {
            var _ = Console.KeyAvailable;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
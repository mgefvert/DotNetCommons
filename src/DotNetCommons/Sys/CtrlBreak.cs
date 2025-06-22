// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys;

/// <summary>
/// Class that captures CTRL-C and CTRL-BREAK keys.
/// </summary>
public static class CtrlBreak
{
    private static readonly ManualResetEvent Event = new(false);
    private static Action? _hook;
    private static bool _hooked;

    private static void CancelKeypress(object? sender, ConsoleCancelEventArgs args)
    {
        args.Cancel = true;
        Event.Set();
        _hook?.Invoke();
    }

    /// <summary>
    /// Perform an action if a break key is pressed.
    /// </summary>
    public static void Hook(Action action)
    {
        _hook = action;
        _hooked = true;
        Console.CancelKeyPress += CancelKeypress;
    }

    /// <summary>
    /// Clear any actions.
    /// </summary>
    public static void Release()
    {
        Console.CancelKeyPress -= CancelKeypress;
        _hooked = false;
        _hook = null;
    }

    /// <summary>
    /// Wait for a break key to be pressed.
    /// </summary>
    public static void WaitFor()
    {
        if (!_hooked)
            Console.CancelKeyPress += CancelKeypress;

        Event.Reset();
        Event.WaitOne();

        if (!_hooked)
            Console.CancelKeyPress -= CancelKeypress;
    }
}
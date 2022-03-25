using System;
using System.Threading;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys;

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

    public static void Hook(Action action)
    {
        _hook = action;
        _hooked = true;
        Console.CancelKeyPress += CancelKeypress;
    }

    public static void Release()
    {
        Console.CancelKeyPress -= CancelKeypress;
        _hooked = false;
        _hook = null;
    }

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
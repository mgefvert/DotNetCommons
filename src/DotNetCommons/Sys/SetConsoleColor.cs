// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys;

/// <summary>
/// Utility function to quickly set a console color and then restore it when it's disposed.
/// E.g.
///
/// using (new SetConsoleColor(ConsoleColor.Yellow))
///    Console.WriteLine("Hello, world!");
/// 
/// </summary>
public class SetConsoleColor : IDisposable
{
    private readonly ConsoleColor _fg;
    private readonly ConsoleColor _bg;

    public SetConsoleColor(ConsoleColor fg, ConsoleColor? bg = null)
    {
        _fg = Console.ForegroundColor;
        _bg = Console.BackgroundColor;
        Console.ForegroundColor = fg;
        if (bg != null)
            Console.BackgroundColor = bg.Value;
    }

    public void Dispose()
    {
        Console.ForegroundColor = _fg;
        Console.BackgroundColor = _bg;
    }
}
using System;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys
{
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
}

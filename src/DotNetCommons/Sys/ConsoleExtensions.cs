﻿using System;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys
{
    public static class ConsoleExtensions
    {
        private static bool? _hasConsole;
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
}

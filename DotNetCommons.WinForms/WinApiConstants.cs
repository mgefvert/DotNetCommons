using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms
{
    // ReSharper disable InconsistentNaming

    public static partial class WinApi
    {
        public const int AC_SRC_ALPHA = 1;
        public const int AC_SRC_OVER = 0;

        public const int LWA_ALPHA = 2;
        public const int LWA_COLORKEY = 1;

        public const int MOD_ALT = 1;
        public const int MOD_CONTROL = 2;
        public const int MOD_SHIFT = 4;
        public const int MOD_WIN = 8;

        public const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;

        public const int STANDARD_RIGHTS_REQUIRED = 0xF0000;

        public const int ULW_ALPHA = 2;
        public const int ULW_COLORKEY = 1;
        public const int ULW_OPAQUE = 4;

        public const int WM_ACTIVATE = 0x0006;
        public const int WM_NCHITTEST = 0x0084;
        public const int WM_POWERBROADCAST = 0x0218;
        public const int WM_HOTKEY = 0x0312;

        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TRANSPARENT = 0x00000020;
    }
}

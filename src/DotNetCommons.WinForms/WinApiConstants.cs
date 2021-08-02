// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global

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

        public const int WS_VISIBLE = 0x1000_0000;
        public const int WS_EX_LAYERED = 0x0008_0000;
        public const int WS_EX_TRANSPARENT = 0x0000_0020;

        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;

        public const int PBT_APMQUERYSUSPEND = 0x0000;
        public const int PBT_APMQUERYSTANDBY = 0x0001;
        public const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
        public const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
        public const int PBT_APMSUSPEND = 0x0004;
        public const int PBT_APMSTANDBY = 0x0005;
        public const int PBT_APMRESUMECRITICAL = 0x0006;
        public const int PBT_APMRESUMESUSPEND = 0x0007;
        public const int PBT_APMRESUMESTANDBY = 0x0008;
        public const int PBT_APMBATTERYLOW = 0x0009;
        public const int PBT_APMPOWERSTATUSCHANGE = 0x000A; // power status
        public const int PBT_APMOEMEVENT = 0x000B;
        public const int PBT_APMRESUMEAUTOMATIC = 0x0012;
        public const int PBT_POWERSETTINGCHANGE = 0x8013; // DPPE
    }
}

using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms
{
    // ReSharper disable InconsistentNaming

    public static partial class WinApi
    {
        public enum ABMsg
        {
            ABM_NEW = 0,
            ABM_REMOVE = 1,
            ABM_QUERYPOS = 2,
            ABM_SETPOS = 3,
            ABM_GETSTATE = 4,
            ABM_GETTASKBARPOS = 5,
            ABM_ACTIVATE = 6,
            ABM_GETAUTOHIDEBAR = 7,
            ABM_SETAUTOHIDEBAR = 8,
            ABM_WINDOWPOSCHANGED = 9,
            ABM_SETSTATE = 10
        }

        public enum ABNotify
        {
            ABN_STATECHANGE = 0,
            ABN_POSCHANGED,
            ABN_FULLSCREENAPP,
            ABN_WINDOWARRANGE
        }

        public enum ABEdge
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        public enum HITTEST
        {
            HTERROR = -2,
            HTTRANSPARENT = -1,
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTSYSMENU = 3,
            HTGROWBOX = 4,
            HTSIZE = 4,
            HTMENU = 5,
            HTHSCROLL = 6,
            HTVSCROLL = 7,
            HTMINBUTTON = 8,
            HTREDUCE = 8,
            HTMAXBUTTON = 9,
            HTZOOM = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTCLOSE = 20,
            HTHELP = 21,
        }

        public enum HWND
        {
            NOTOPMOST = -2,
            TOPMOST = -1,
            TOP = 0,
            BOTTOM = 1
        }

        [Flags]
        public enum ScmAccessRights
        {
            Connect = 0x0001,
            CreateService = 0x0002,
            EnumerateService = 0x0004,
            Lock = 0x0008,
            QueryLockStatus = 0x0010,
            ModifyBootConfig = 0x0020,
            StandardRightsRequired = 0xF0000,
            AllAccess = StandardRightsRequired | Connect | CreateService | EnumerateService |
                        Lock | QueryLockStatus | ModifyBootConfig
        }

        [Flags]
        public enum ServiceAccessRights
        {
            QueryConfig = 0x1,
            ChangeConfig = 0x2,
            QueryStatus = 0x4,
            EnumerateDependants = 0x8,
            Start = 0x10,
            Stop = 0x20,
            PauseContinue = 0x40,
            Interrogate = 0x80,
            UserDefinedControl = 0x100,
            Delete = 0x00010000,
            StandardRightsRequired = 0xF0000,
            AllAccess = StandardRightsRequired | QueryConfig | ChangeConfig | QueryStatus |
                        EnumerateDependants | Start | Stop | PauseContinue | Interrogate | UserDefinedControl
        }

        public enum ServiceBootFlag
        {
            Start = 0,
            SystemStart = 1,
            AutoStart = 2,
            DemandStart = 3,
            Disabled = 4
        }

        public enum ServiceControl
        {
            Stop = 1,
            Pause = 2,
            Continue = 3,
            Interrogate = 4,
            Shutdown = 5,
            ParamChange = 6,
            NetBindAdd = 7,
            NetBindRemove = 8,
            NetBindEnable = 9,
            NetBindDisable = 10
        }

        public enum ServiceError
        {
            Ignore = 0,
            Normal = 1,
            Severe = 2,
            Critical = 3
        }

        public enum ServiceState
        {
            Unknown = -1, // The state cannot be (has not been) retrieved.
            NotFound = 0, // The service is not known on the host server.
            Stopped = 1,
            StartPending = 2,
            StopPending = 3,
            Running = 4,
            ContinuePending = 5,
            PausePending = 6,
            Paused = 7
        }

        public enum SPI
        {
            GETSCREENSAVEACTIVE = 0x0010,
            SETDESKWALLPAPER = 0x0014,
            GETDESKWALLPAPER = 0x0073
        }

        [Flags]
        public enum SPIF
        {
            None = 0x00,
            SPIF_UPDATEINIFILE = 0x01,
            SPIF_SENDCHANGE = 0x02,
            SPIF_SENDWININICHANGE = 0x02
        }

        public enum SWP
        {
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000
        }
    }
}

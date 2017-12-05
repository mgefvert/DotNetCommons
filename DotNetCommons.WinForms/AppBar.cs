using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// ReSharper disable InconsistentNaming

namespace DotNetCommons.WinForms
{
    public class AppBarForm : Form
    {
        public bool Registered { get; private set; }
        private int uCallBack;

        public void RegisterAppBar()
        {
            var abd = new WinApi.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = Handle;

            uCallBack = WinApi.RegisterWindowMessage("AppBarMessage");
            abd.uCallbackMessage = uCallBack;

            WinApi.SHAppBarMessage((int)WinApi.ABMsg.ABM_NEW, ref abd);
            Registered = true;
            ABSetPos();
        }

        public void UnregisterAppBar()
        {
            var abd = new WinApi.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = Handle;

            WinApi.SHAppBarMessage((int)WinApi.ABMsg.ABM_REMOVE, ref abd);
            Registered = false;
        }

        protected void ABSetPos()
        {
            var abd = new WinApi.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = Handle;
            abd.uEdge = (int)WinApi.ABEdge.ABE_TOP;

            if (abd.uEdge == (int)WinApi.ABEdge.ABE_LEFT || abd.uEdge == (int)WinApi.ABEdge.ABE_RIGHT)
            {
                abd.rc.top = 0;
                abd.rc.bottom = SystemInformation.PrimaryMonitorSize.Height;
                if (abd.uEdge == (int)WinApi.ABEdge.ABE_LEFT)
                {
                    abd.rc.left = 0;
                    abd.rc.right = Size.Width;
                }
                else
                {
                    abd.rc.right = SystemInformation.PrimaryMonitorSize.Width;
                    abd.rc.left = abd.rc.right - Size.Width;
                }
            }
            else
            {
                abd.rc.left = 0;
                abd.rc.right = SystemInformation.PrimaryMonitorSize.Width;
                if (abd.uEdge == (int)WinApi.ABEdge.ABE_TOP)
                {
                    abd.rc.top = 0;
                    abd.rc.bottom = Size.Height;
                }
                else
                {
                    abd.rc.bottom = SystemInformation.PrimaryMonitorSize.Height;
                    abd.rc.top = abd.rc.bottom - Size.Height;
                }
            }

            // Query the system for an approved size and position. 
            WinApi.SHAppBarMessage((int)WinApi.ABMsg.ABM_QUERYPOS, ref abd);

            // Adjust the rectangle, depending on the edge to which the 
            // appbar is anchored. 
            switch (abd.uEdge)
            {
                case (int)WinApi.ABEdge.ABE_LEFT:
                    abd.rc.right = abd.rc.left + Size.Width;
                    break;
                case (int)WinApi.ABEdge.ABE_RIGHT:
                    abd.rc.left = abd.rc.right - Size.Width;
                    break;
                case (int)WinApi.ABEdge.ABE_TOP:
                    abd.rc.bottom = abd.rc.top + Size.Height;
                    break;
                case (int)WinApi.ABEdge.ABE_BOTTOM:
                    abd.rc.top = abd.rc.bottom - Size.Height;
                    break;
            }

            // Pass the final bounding rectangle to the system. 
            WinApi.SHAppBarMessage((int)WinApi.ABMsg.ABM_SETPOS, ref abd);

            // Move and size the appbar so that it conforms to the 
            // bounding rectangle passed to the system. 
            WinApi.MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == uCallBack && m.WParam.ToInt32() == (int)WinApi.ABNotify.ABN_POSCHANGED)
                ABSetPos();

            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style &= (~0x00C00000); // WS_CAPTION
                cp.Style &= (~0x00800000); // WS_BORDER
                cp.ExStyle = 0x00000080 | 0x00000008; // WS_EX_TOOLWINDOW | WS_EX_TOPMOST
                return cp;
            }
        }
    }
}
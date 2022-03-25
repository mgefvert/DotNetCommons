using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// ReSharper disable InconsistentNaming

namespace DotNetCommons.WinForms;

public class AppBarForm : Form
{
    protected WinApi.ABEdge AppBarPosition { get; set; } = WinApi.ABEdge.ABE_TOP;
    protected Screen AppBarMonitor { get; set; }
    private int uCallBack;
    private bool _inSetPos;

    public bool Registered { get; private set; }

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
        if (_inSetPos)
            return;

        _inSetPos = true;
        try
        {
            var abd = new WinApi.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = Handle;
            abd.uEdge = (int)AppBarPosition;

            CalculateCoordinates(ref abd, AppBarMonitor);

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
        finally
        {
            _inSetPos = false;
        }
    }

    private void CalculateCoordinates(ref WinApi.APPBARDATA result, Screen screen)
    {
        if (screen == null)
            screen = Screen.PrimaryScreen;

        if (result.uEdge == (int)WinApi.ABEdge.ABE_LEFT || result.uEdge == (int)WinApi.ABEdge.ABE_RIGHT)
        {
            result.rc.top = screen.WorkingArea.Top;
            result.rc.bottom = screen.WorkingArea.Bottom;
            if (result.uEdge == (int)WinApi.ABEdge.ABE_LEFT)
            {
                result.rc.left = screen.WorkingArea.Left;
                result.rc.right = Size.Width;
            }
            else
            {
                result.rc.right = screen.WorkingArea.Right;
                result.rc.left = result.rc.right - Size.Width;
            }
        }
        else
        {
            result.rc.left = screen.WorkingArea.Left;
            result.rc.right = screen.WorkingArea.Right;
            if (result.uEdge == (int)WinApi.ABEdge.ABE_TOP)
            {
                result.rc.top = screen.WorkingArea.Top;
                result.rc.bottom = Size.Height;
            }
            else
            {
                result.rc.bottom = screen.WorkingArea.Height;
                result.rc.top = result.rc.bottom - Size.Height;
            }
        }
    }

    protected override void WndProc(ref Message m)
    {
        //if (m.Msg == uCallBack && m.WParam.ToInt32() == (int)WinApi.ABNotify.ABN_POSCHANGED)
        //    ABSetPos();

        base.WndProc(ref m);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            // cp.Style &= ~0x00C00000; // WS_CAPTION
            // cp.Style &= ~0x00800000; // WS_BORDER
            cp.ExStyle = 0x00000080 | 0x00000008; // WS_EX_TOOLWINDOW | WS_EX_TOPMOST
            return cp;
        }
    }
}
﻿using Timer = System.Windows.Forms.Timer;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms;

public class AlphaForm : Form
{
    private readonly Timer _timer;
    protected int PreferredScreen { get; set; }

    public int Alpha { get; set; }
    public Bitmap CurrentBitmap { get; set; }
    public int TargetAlpha { get; set; }
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
    public AnchorStyles AnchorOffset { get; set; } = AnchorStyles.Left | AnchorStyles.Top;
    
    public AlphaForm() : this(false)
    {
    }

    public AlphaForm(bool showInTaskBar)
    {
        ShowIcon = showInTaskBar;
        ShowInTaskbar = showInTaskBar;
        FormBorderStyle = FormBorderStyle.None;

        _timer = new Timer();
        _timer.Tick += TimerOnTick;
        _timer.Interval = 10;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            if (!DesignMode)
                cp.ExStyle |= WinApi.WS_EX_LAYERED;// | WinApi.WS_EX_TRANSPARENT;
            return cp;
        }
    }

    protected void FadeIn(int alpha)
    {
        TargetAlpha = alpha;
        Alpha = 0;
        UpdateAlpha();
        Show();
        UpdateImage();

        _timer.Start();
    }

    protected void FadeOut()
    {
        TargetAlpha = 0;
        _timer.Start();
        WaitTimer();
        Hide();
    }

    protected void FadeTo(int alpha)
    {
        if (TargetAlpha == alpha)
            return;

        TargetAlpha = alpha;
        if (!_timer.Enabled)
            _timer.Start();
    }

    private void TimerOnTick(object sender, EventArgs eventArgs)
    {
        if (Alpha == TargetAlpha)
        {
            _timer.Stop();
            return;
        }

        Alpha = Alpha < TargetAlpha
            ? Math.Min(TargetAlpha, Alpha + 8)
            : Math.Max(TargetAlpha, Alpha - 8);
        UpdateAlpha();
    }

    protected void InvokeOnMainThread(Action action)
    {
        if (Thread.CurrentThread.IsThreadPoolThread || Thread.CurrentThread.IsBackground)
            Invoke(action);
        else
            action();
    }
    
    public void UpdateAlpha()
    {
        var blend = new WinApi.BLENDFUNCTION
        {
            BlendOp = WinApi.AC_SRC_OVER,
            BlendFlags = 0,
            SourceConstantAlpha = (byte)Alpha,
            AlphaFormat = WinApi.AC_SRC_ALPHA
        };

        var zero = IntPtr.Zero;
        InvokeOnMainThread(() => WinApi.UpdateLayeredWindow(Handle, zero, zero, zero, zero, zero, 0, ref blend, WinApi.ULW_ALPHA));
    }

    public bool UpdateImage()
    {
        if (CurrentBitmap == null)
            return false;

        var area = (Screen.AllScreens.ElementAtOrDefault(PreferredScreen) ?? Screen.PrimaryScreen).WorkingArea;

        Width = CurrentBitmap.Width;
        Height = CurrentBitmap.Height;

        int left = 0;
        int top = 0;
        if (AnchorOffset.HasFlag(AnchorStyles.Left))
            left = area.Left;
        else if (AnchorOffset.HasFlag(AnchorStyles.Right))
            left = area.Right - Width;
        if (AnchorOffset.HasFlag(AnchorStyles.Top))
            top = area.Top;
        else if (AnchorOffset.HasFlag(AnchorStyles.Right))
            top = area.Bottom - Height;
        Left = left + OffsetX;
        Top = top + OffsetY;
        
        var screenDc = WinApi.GetDC(IntPtr.Zero);
        var memDc = WinApi.CreateCompatibleDC(screenDc);

        // Display-image
        var hBitmap = CurrentBitmap.GetHbitmap(Color.FromArgb(0));  // Set the fact that background is transparent
        var oldBitmap = WinApi.SelectObject(memDc, hBitmap);

        // Display-rectangle
        var size = CurrentBitmap.Size;
        var pointSource = new Point(0, 0);
        var topPos = new Point(Left, Top);

        // Set up blending options
        var blend = new WinApi.BLENDFUNCTION
        {
            BlendOp = WinApi.AC_SRC_OVER,
            BlendFlags = 0,
            SourceConstantAlpha = (byte)Alpha,
            AlphaFormat = WinApi.AC_SRC_ALPHA
        };

        var result = false;
        InvokeOnMainThread(() => result = WinApi.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, WinApi.ULW_ALPHA));

        // Clean-up
        WinApi.ReleaseDC(IntPtr.Zero, screenDc);
        if (hBitmap != IntPtr.Zero)
        {
            WinApi.SelectObject(memDc, oldBitmap);
            WinApi.DeleteObject(hBitmap);
        }

        WinApi.DeleteDC(memDc);

        return result;
    }

    private void WaitTimer()
    {
        while (_timer.Enabled)
        {
            Thread.Sleep(1);
            Application.DoEvents();
        }
    }
}
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms.Graphics;

public class BitmapBuffer : IDisposable
{
    public Bitmap Bitmap;
    public int[] Buffer;
    protected BitmapData Data;
    public int Length;
    public int Width;
    public int Height;

    public BitmapBuffer(Bitmap bitmap)
    {
        Bitmap = bitmap;
        Width = bitmap.Width;
        Height = bitmap.Height;
        Length = Width * Height;

        Data = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        Buffer = new int[Length];
        Marshal.Copy(Data.Scan0, Buffer, 0, Length);
    }

    public void Dispose()
    {
        Marshal.Copy(Buffer, 0, Data.Scan0, Length);
        Bitmap.UnlockBits(Data);

        Bitmap = null;
        Data = null;
        Buffer = null;
        Length = 0;
    }

    public int GetPixel(int offset)
    {
        if (offset < 0)
            return 0;

        return Buffer[offset];
    }

    public byte GetPixelAlpha(int offset)
    {
        if (offset < 0)
            return 0;

        return (byte)(Buffer[offset] >> 24);
    }

    public void SetPixel(int offset, int color)
    {
        if (offset < 0)
            return;

        Buffer[offset] = color;
    }

    public void SetPixelAlpha(int offset, byte alpha)
    {
        if (offset < 0)
            return;

        Buffer[offset] = Buffer[offset] & 0xFFFFFF | (alpha << 24);
    }

    public void AdjustPixelMaxAlpha(int offset, byte alpha)
    {
        if (offset < 0)
            return;

        var color = Buffer[offset];
        var pixelAlpha = (color >> 24) & 0xFF;
        var value = alpha + pixelAlpha;
        if (value > 255)
            value = 255;

        Buffer[offset] = (color & 0xFFFFFF) | (value << 24);
    }

    public void MultPixelAlpha(int offset, float multiplier)
    {
        if (offset < 0)
            return;

        var color = Buffer[offset];
        var alpha = ((color >> 24) & 0xFF) * multiplier;
        Buffer[offset] = (color & 0xFFFFFF) | ((byte)alpha << 24);
    }

    public int CoordToOffset(int x, int y)
    {
        return (x >= 0 && x < Width && y >= 0 && y < Height) ? y * Data.Width + x : -1;
    }

    public Point OffsetToCoord(int offset)
    {
        return offset < 0 || offset >= Length
            ? Point.Empty
            : new Point(offset % Data.Width, offset / Data.Width);
    }

    public int PercentMaxAlpha()
    {
        var count = 0;
        for (var i = 0; i < Length; i++)
            if ((byte)(Buffer[i] >> 24) > 250)
                count++;

        return 100 * count / Length;
    }
}
using System.Drawing.Imaging;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms.Graphics;

public class BitmapBuffer : IDisposable
{
    private static readonly PixelFormat[] SupportedPixelFormats = [PixelFormat.Format24bppRgb, PixelFormat.Format32bppArgb];

    public Bitmap Bitmap { get; }
    public BitmapData BitmapData { get; }
    public Rectangle Rectangle { get; }
    public int Height { get; }
    public int Width { get; }
    public bool IsDisposed { get; private set; }

    /// Represents a memory buffer for a bitmap, providing utilities to manipulate and access pixel data.
    /// This class allows locking a bitmap's memory for pixel-level operations and ensures proper cleanup
    /// to release resources when no longer needed.
    public BitmapBuffer(Bitmap bitmap, ImageLockMode lockMode, PixelFormat pixelFormat)
    {
        if (!SupportedPixelFormats.Contains(pixelFormat))
            throw new NotSupportedException($"PixelFormat {pixelFormat} not supported");

        Bitmap     = bitmap;
        Rectangle  = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        BitmapData = bitmap.LockBits(Rectangle, lockMode, pixelFormat);
        Height     = BitmapData.Height;
        Width      = BitmapData.Width;

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Width, nameof(Width));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Height, nameof(Height));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Width, ushort.MaxValue, nameof(Width));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Height, ushort.MaxValue, nameof(Height));
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        Bitmap.UnlockBits(BitmapData);
    }

    /// Retrieves a scanline from the bitmap buffer at the specified y-coordinate.
    /// <param name="y">
    /// The y-coordinate of the scanline to retrieve. If outside the bounds of the bitmap height, a dummy scanline handler will be returned
    /// that always returns Color.Empty.
    /// </param>
    public ScanLine GetScanline(int y)
    {
        if (IsDisposed || y < 0 && y >= Height)
            return new ScanLine(IntPtr.Zero, 0, ScanLineType.Empty);

        var rowStart = BitmapData.Scan0 + y * BitmapData.Stride;
        return BitmapData.PixelFormat switch
        {
            PixelFormat.Format24bppRgb  => new ScanLine(rowStart, (ushort)Width, ScanLineType.Rgb24),
            PixelFormat.Format32bppArgb => new ScanLine(rowStart, (ushort)Width, ScanLineType.Argb32),
            _                           => throw new NotSupportedException()
        };
    }

    /// Executes an action for every scan line in the bitmap buffer.
    /// <param name="action">
    /// The action to execute for each pixel. The action receives the current scanline, and the y-coordinate.
    /// </param>
    public void ForAllLines(Action<ScanLine, int> action)
    {
        for (var y = 0; y < Height; y++)
            action(GetScanline(y), y);
    }

    /// Executes an action for every pixel in the bitmap buffer.
    /// <param name="action">
    /// The action to execute for each pixel. The action receives the current scanline, the pixel's x-coordinate,
    /// and the pixel's y-coordinate.
    /// </param>
    public void ForAllPixels(Action<ScanLine, int, int> action)
    {
        for (var y = 0; y < Height; y++)
        {
            var scanLine = GetScanline(y);
            for (var x = 0; x < Width; x++)
                action(scanLine, x, y);
        }
    }

    /// Gets the alpha value of a specific pixel in the bitmap buffer. Pixels outside the bitmap area return 0.
    public byte GetAlpha(int x, int y)
    {
        return GetScanline(y).GetAlpha(x);
    }

    /// Gets the color of a specific pixel in the bitmap buffer, including any alpha value. Pixels outside the bitmap area return
    /// Color.Empty.
    public Color GetColor(int x, int y)
    {
        return GetScanline(y).GetColor(x);
    }

    /// Sets the alpha value of a specific pixel in the bitmap buffer. Pixels outside the bitmap area are silently discarded.
    /// If the bitmap doesn't support Alpha layers, the operation is ignored. Alpha 0 is transparent, 255 fully opaque.
    public void SetAlpha(int x, int y, byte alpha)
    {
        GetScanline(y).SetAlpha(x, alpha);
    }

    /// Sets the color of a specific pixel in the bitmap buffer. Pixels outside the bitmap area are silently discarded.
    public void SetColor(int x, int y, Color color)
    {
        GetScanline(y).SetColor(x, color);
    }
}
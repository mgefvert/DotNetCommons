using System.Runtime.CompilerServices;

namespace DotNetCommons.WinForms.Graphics;

public enum ScanLineType : byte
{
    Empty,
    Rgb24,
    Argb32
}

public readonly struct ScanLine
{
    private readonly IntPtr _start;
    private readonly ushort _width;
    private readonly ScanLineType _type;
    private readonly byte _size;

    public ScanLine(IntPtr start, ushort width, ScanLineType type)
    {
        _start = start;
        _type  = type;
        _width = width;
        _size = type switch
        {
            ScanLineType.Empty  => 0,
            ScanLineType.Rgb24  => 3,
            ScanLineType.Argb32 => 4,
            _                   => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe byte* UnsafeGetX(int x)
    {
        return (byte*)(_start + x * _size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe byte UnsafeReadAlpha(ScanLineType type, byte* ptr)
    {
        return type switch
        {
            ScanLineType.Rgb24  => 255,
            ScanLineType.Argb32 => ptr[3],
            _                   => 0
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Color UnsafeReadColor(ScanLineType type, byte* ptr)
    {
        return type switch
        {
            ScanLineType.Rgb24  => Color.FromArgb(ptr[2], ptr[1], ptr[0]),
            ScanLineType.Argb32 => Color.FromArgb(*(int*)ptr),
            _                   => Color.Empty
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void UnsafeWriteAlpha(ScanLineType type, byte* ptr, byte alpha)
    {
        if (type == ScanLineType.Argb32)
            ptr[3] = alpha;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void UnsafeWriteColor(ScanLineType type, byte* ptr, Color color)
    {
        switch (type)
        {
            case ScanLineType.Rgb24:
                ptr[0] = color.B;
                ptr[1] = color.G;
                ptr[2] = color.R;
                break;
            case ScanLineType.Argb32:
                *(int*)ptr = color.ToArgb();
                break;
        }
    }

    public unsafe void AddAlpha(int x, byte alpha)
    {
        if (x < 0 || x >= _width)
            return;

        var ptr   = UnsafeGetX(x);
        var value = Math.Clamp(UnsafeReadAlpha(_type, ptr) + alpha, 0, 255);
        UnsafeWriteAlpha(_type, ptr, (byte)value);
    }

    public unsafe void AddColor(int x, Color color)
    {
        if (x < 0 || x >= _width)
            return;

        var ptr      = UnsafeGetX(x);
        var existing = UnsafeReadColor(_type, ptr);
        var newColor = Color.FromArgb(
            Math.Clamp(existing.A + color.A, 0, 255),
            Math.Clamp(existing.R + color.R, 0, 255),
            Math.Clamp(existing.G + color.G, 0, 255),
            Math.Clamp(existing.B + color.B, 0, 255)
        );
        UnsafeWriteColor(_type, ptr, newColor);
    }

    public unsafe void BlendColor(int x, Color color, float amount)
    {
        if (x < 0 || x >= _width)
            return;

        amount = Math.Clamp(amount, 0, 1);

        var ptr      = UnsafeGetX(x);
        var existing = UnsafeReadColor(_type, ptr);
        var newColor = Color.FromArgb(
            (byte)(existing.A + (color.A - existing.A) * amount),
            (byte)(existing.R + (color.R - existing.R) * amount),
            (byte)(existing.G + (color.G - existing.G) * amount),
            (byte)(existing.B + (color.B - existing.B) * amount)
        );
        UnsafeWriteColor(_type, ptr, newColor);
    }

    public unsafe byte GetAlpha(int x)
    {
        return x >= 0 && x < _width
            ? UnsafeReadAlpha(_type, UnsafeGetX(x))
            : (byte)0;
    }

    public unsafe Color GetColor(int x)
    {
        return x >= 0 && x < _width
            ? UnsafeReadColor(_type, UnsafeGetX(x))
            : Color.Empty;
    }

    public unsafe void MultiplyAlpha(int x, float multiplier)
    {
        if (x < 0 || x >= _width)
            return;

        var ptr   = UnsafeGetX(x);
        var value = Math.Clamp(UnsafeReadAlpha(_type, ptr) * multiplier, 0, 255);
        UnsafeWriteAlpha(_type, ptr, (byte)value);
    }

    public unsafe void SetAlpha(int x, byte alpha)
    {
        if (x >= 0 && x < _width)
            UnsafeWriteAlpha(_type, UnsafeGetX(x), alpha);
    }

    public unsafe void SetColor(int x, Color color)
    {
        if (x >= 0 && x < _width)
            UnsafeWriteColor(_type, UnsafeGetX(x), color);
    }
}
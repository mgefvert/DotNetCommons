

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms.Graphics;

public class PaintBrush
{
    private readonly byte[,] _brush;

    public PaintBrush(int size, byte opacity)
    {
        var half = size / 2;
        _brush = new byte[size, size];

        for (var x = 0; x < size; x++)
        {
            var xh2 = (x - half) * (x - half);
            for (var y = 0; y < size; y++)
            {
                var yh2 = (y - half) * (y - half);

                var dist = 1 - Math.Sqrt(xh2 + yh2) / half;
                if (dist > 0 && dist <= 1)
                    _brush[x, y] = (byte)(opacity * dist);
            }
        }
    }

    public void Reveal(BitmapBuffer buffer, int x, int y)
    {
        var length = _brush.GetLength(0);
        var half = length / 2;

        for (var dy = 0; dy < length; dy++)
        for (var dx = 0; dx < length; dx++)
        {
            var offset = buffer.CoordToOffset(x + dx - half, y + dy - half);
            buffer.AdjustPixelMaxAlpha(offset, _brush[dx, dy]);
        }
    }
}
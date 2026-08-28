using System.Drawing;

namespace DotNetCommons;

public static class CommonDrawingExtensions
{
    public static void Invert(this PointF point)
    {
        point.X = -point.X;
        point.Y = -point.Y;
    }

    public static void Scale(this RectangleF rect, float scaleX, float scaleY)
    {
        rect.Width  *= scaleX;
        rect.Height *= scaleY;
    }

    public static void ScaleToMin(this RectangleF rect, SizeF minSize)
    {
        var scaleX = minSize.Width / rect.Width;
        var scaleY = minSize.Height / rect.Height;
        rect.Scale(Math.Max(scaleX, scaleY), Math.Max(scaleX, scaleY));
    }

    public static void ScaleToMax(this RectangleF rect, SizeF maxSize)
    {
        var scaleX = maxSize.Width / rect.Width;
        var scaleY = maxSize.Height / rect.Height;
        rect.Scale(Math.Min(scaleX, scaleY), Math.Min(scaleX, scaleY));
    }
}
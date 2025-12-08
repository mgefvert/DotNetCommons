using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DotNetCommons.WinForms.Graphics;

public static class BitmapOperations
{
    /// <summary>
    /// Converts an image to a bitmap with the specified pixel format, optionally forcing conversion (cloning to a new bitmap) even if the
    /// image is already a bitmap with the desired format.
    /// </summary>
    /// <param name="source">The source image to be converted.</param>
    /// <param name="pixelFormat">The pixel format of the resulting bitmap.</param>
    /// <param name="force">Specifies whether to force the conversion even if the source image is already a bitmap with the target pixel format.</param>
    /// <returns>A new bitmap with the specified pixel format, containing the visual content of the original image.</returns>
    public static Bitmap ConvertToBitmap(this Image source, PixelFormat pixelFormat, bool force)
    {
        if (source is Bitmap bmp && source.PixelFormat == pixelFormat && force == false)
            return bmp;

        var bitmap = CreateNewBitmapLike(source, pixelFormat, source.Size);

        using var g = System.Drawing.Graphics.FromImage(bitmap);
        g.DrawImage(source, 0, 0);

        return bitmap;
    }

    /// <summary>
    /// Creates a new bitmap with the same resolution and property items as the original image, but with a specified size and pixel format.
    /// </summary>
    /// <param name="source">The original image to copy resolution and property items from.</param>
    /// <param name="pixelFormat">The pixel format of the new bitmap.</param>
    /// <param name="size">The size of the new bitmap.</param>
    /// <returns>A new bitmap that has the specified size and pixel format, with the same resolution and property items as the
    ///     original image.</returns>
    public static Bitmap CreateNewBitmapLike(Image source, PixelFormat pixelFormat, Size size)
    {
        var result = new Bitmap(size.Width, size.Height, pixelFormat);
        result.SetResolution(source.HorizontalResolution, source.VerticalResolution);

        foreach (var id in source.PropertyIdList)
        {
            var propertyItem = source.GetPropertyItem(id);
            if (propertyItem != null)
                result.SetPropertyItem(propertyItem);
        }

        return result;
    }

    /// <summary>
    /// Crops a specified rectangular region from the source bitmap and creates a new bitmap with the defined dimensions.
    /// </summary>
    /// <param name="source">The source bitmap to be cropped.</param>
    /// <param name="rect">The rectangular region defining the area to crop.</param>
    /// <returns>A new bitmap containing the cropped region.</returns>
    public static Bitmap Crop(this Bitmap source, Rectangle rect)
    {
        var result = CreateNewBitmapLike(source, source.PixelFormat, rect.Size);

        using var g = System.Drawing.Graphics.FromImage(result);
        g.DrawImage(source, 0, 0, rect, GraphicsUnit.Pixel);

        return result;
    }

    /// <summary>
    /// Locks a bitmap's buffer and provides access to pixel data in the specified lock mode.
    /// </summary>
    /// <param name="bitmap">The bitmap for which the buffer will be locked.</param>
    /// <param name="lockMode">The locking mode that specifies the access level for the locked bitmap.</param>
    /// <returns>An instance of BitmapBuffer that represents the locked buffer with access to pixel data.</returns>
    public static BitmapBuffer LockBuffer(this Bitmap bitmap, ImageLockMode lockMode)
    {
        return new BitmapBuffer(bitmap, lockMode, bitmap.PixelFormat);
    }

    /// <summary>
    /// Resamples the provided bitmap to a new width and height using the specified interpolation mode.
    /// </summary>
    /// <param name="source">The original bitmap to be resampled.</param>
    /// <param name="size">The desired size of the resampled bitmap.</param>
    /// <param name="interpolationMode">The interpolation mode to use for resampling, with a default value of HighQualityBicubic.</param>
    /// <returns>A new bitmap that represents the resampled version of the original image with the specified dimensions
    ///     and interpolation mode.</returns>
    public static Bitmap Resample(this Bitmap source, Size size,
        InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic)
    {
        var result = CreateNewBitmapLike(source, source.PixelFormat, size);

        using var g = System.Drawing.Graphics.FromImage(result);

        var gu = GraphicsUnit.Pixel;
        g.InterpolationMode = interpolationMode;
        g.DrawImage(source, result.GetBounds(ref gu), source.GetBounds(ref gu), gu);

        return result;
    }

    /// <summary>
    /// Scales the provided bitmap proportionally to fit within the specified bounds, maintaining the aspect ratio.
    /// </summary>
    /// <param name="source">The original bitmap to be scaled.</param>
    /// <param name="bounds">The maximum bounds within which the bitmap should fit.</param>
    /// <returns>A new bitmap that is scaled proportionally to fit within the specified bounds. If the size is unchanged,
    ///     the original bitmap is returned.</returns>
    public static Bitmap ScaleToFit(this Bitmap source, Size bounds)
    {
        var newSize = ScaleToFit(source.Size, bounds);
        return newSize == source.Size
            ? source
            : Resample(source, newSize);
    }

    /// <summary>
    /// Scales the original Size object proportionally to fit within the specified bounds, maintaining the aspect ratio.
    /// </summary>
    /// <param name="original">The original size to be scaled.</param>
    /// <param name="bounds">The maximum bounds within which the size should fit.</param>
    /// <returns>A new size that is scaled proportionally to fit within the specified bounds.</returns>
    public static Size ScaleToFit(Size original, Size bounds)
    {
        var dx    = (double)bounds.Width / original.Width;
        var dy    = (double)bounds.Height / original.Height;
        var delta = Math.Min(dx, dy);

        return new Size(
            (int)Math.Round(original.Width * delta),
            (int)Math.Round(original.Height * delta)
        );
    }

    /// <summary>
    /// Scales the provided bitmap proportionally to at least cover the specified bounds, maintaining the aspect ratio.
    /// </summary>
    /// <param name="source">The original bitmap to be scaled.</param>
    /// <param name="bounds">The minimum bounds that the bitmap should cover.</param>
    /// <returns>A new bitmap that is scaled proportionally to at least cover the specified bounds. If the size is unchanged,
    ///     the original bitmap is returned.</returns>
    public static Bitmap ScaleToCover(this Bitmap source, Size bounds)
    {
        var newSize = ScaleToCover(source.Size, bounds);
        return newSize == source.Size
            ? source
            : Resample(source, newSize);
    }

    /// <summary>
    /// Scales the original Size object proportionally to at least cover the specified bounds, maintaining the aspect ratio.
    /// </summary>
    /// <param name="original">The original size to be scaled.</param>
    /// <param name="bounds">The minimum bounds that the size should cover.</param>
    /// <returns>A new size that is scaled proportionally to at least cover the specified bounds.</returns>
    public static Size ScaleToCover(Size original, Size bounds)
    {
        var dx    = (double)bounds.Width / original.Width;
        var dy    = (double)bounds.Height / original.Height;
        var delta = Math.Max(dx, dy);

        return new Size(
            (int)Math.Round(original.Width * delta),
            (int)Math.Round(original.Height * delta)
        );
    }

    /// <summary>
    /// Creates a thumbnail of the specified bitmap, scaling it to fit within the given size while maintaining its aspect ratio,
    /// and fills the remaining space with the specified background color.
    /// </summary>
    /// <param name="source">The original bitmap to be used for creating the thumbnail.</param>
    /// <param name="size">The target size of the thumbnail.</param>
    /// <param name="background">The background color to fill the space outside the scaled image.</param>
    /// <returns>A new bitmap representing the thumbnail with the specified size and background color.</returns>
    public static Bitmap Thumbnail(this Bitmap source, Size size, Color background)
    {
        using var resampled = ScaleToFit(source, size);

        // Make another clone of the new image and set the target size
        var result = CreateNewBitmapLike(source, source.PixelFormat, size);

        using var graphics = System.Drawing.Graphics.FromImage(result);
        using var brush    = new SolidBrush(background);

        graphics.FillRectangle(brush, graphics.ClipBounds);
        graphics.InterpolationMode = InterpolationMode.Default;
        graphics.DrawImageUnscaled(resampled, (result.Width - resampled.Width) / 2, (result.Height - resampled.Height) / 2);

        return result;
    }

    /// <summary>
    /// Writes a bitmap image to a specified stream using the given image format and encoder parameters.
    /// </summary>
    /// <param name="bitmap">The bitmap image to be written to the stream.</param>
    /// <param name="stream">The stream to which the bitmap will be written.</param>
    /// <param name="imageFormat">The format in which the bitmap will be saved.</param>
    /// <param name="encoderParams">Optional encoder parameters to customize the image saving process.</param>
    public static void WriteToStream(this Bitmap bitmap, Stream stream, ImageFormat imageFormat, EncoderParameters encoderParams = null)
    {
        var codec = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == imageFormat.Guid);
        bitmap.Save(stream, codec, encoderParams);
    }
}
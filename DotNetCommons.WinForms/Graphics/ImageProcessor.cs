using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms.VisualStyles;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms.Graphics
{
    public class ImageProcessor
    {
        public delegate bool ColorMatchDelegate(Color color);

        private Bitmap _bitmap;
        private readonly Random _rnd = new Random();

        public Bitmap Bitmap => _bitmap;
        public int Height => _bitmap.Height;
        public int Width => _bitmap.Width;
        public int PixelCount => Height * Width;
        public Size Size => new Size(_bitmap.Width, _bitmap.Height);
        public InterpolationMode InterpolationMode { get; set; }

        public ImageProcessor(Image image)
        {
            _bitmap = ConvertToBitmap(image, image.PixelFormat);
            InterpolationMode = InterpolationMode.HighQualityBicubic;
        }

        public ImageProcessor(Image image, PixelFormat convertTo) : this(image)
        {
            ConvertToFormat(convertTo);
        }

        public Stream AsBmp() => AsStream(ImageFormat.Bmp);
        public Stream AsJpeg() => AsStream(ImageFormat.Jpeg);
        public Stream AsPng() => AsStream(ImageFormat.Png);

        public Stream AsStream(ImageFormat format)
        {
            var result = new MemoryStream();
            _bitmap.Save(result, format);
            result.Position = 0;
            return result;
        }

        private static Bitmap ConvertToBitmap(Image image, PixelFormat format)
        {
            if (image is Bitmap && image.PixelFormat == format)
                return (Bitmap)image;

            var bitmap = CreateNewBitmap(image, format, image.Size);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
                g.DrawImage(image, 0, 0);

            return bitmap;
        }

        public void ConvertToFormat(PixelFormat format)
        {
            var newBitmap = ConvertToBitmap(_bitmap, format);
            if (_bitmap == newBitmap)
                return;

            _bitmap.Dispose();
            _bitmap = newBitmap;
        }

        public static Bitmap CreateNewBitmap(Image original, PixelFormat pixelFormat, Size size)
        {
            var result = new Bitmap(size.Width, size.Height, pixelFormat);
            foreach (var id in original.PropertyIdList)
                result.SetPropertyItem(original.GetPropertyItem(id));
            return result;
        }

        public void FadeEdges(int percent, Edges edges)
        {
            ConvertToFormat(PixelFormat.Format32bppArgb);

            var w = _bitmap.Width;
            var h = _bitmap.Height;

            var el = edges.HasFlag(Edges.Left);
            var er = edges.HasFlag(Edges.Right);
            var et = edges.HasFlag(Edges.Top);
            var eb = edges.HasFlag(Edges.Bottom);

            using (var buffer = GetBitmapBuffer())
            {
                var dim = h / percent;
                if (el || er)
                    for (var y = 0; y < dim; y++)
                    {
                        var c = y / (float)dim;
                        for (var x = 0; x < w; x++)
                        {
                            if (el)
                                buffer.MultPixelAlpha(buffer.CoordToOffset(x, y), c);
                            if (er)
                                buffer.MultPixelAlpha(buffer.CoordToOffset(x, h - y - 1), c);
                        }
                    }

                dim = _bitmap.Width / percent;
                if (et || eb)
                    for (var x = 0; x < dim; x++)
                    {
                        var c = x / (float)dim;
                        for (var y = 0; y < h; y++)
                        {
                            if (et)
                                buffer.MultPixelAlpha(buffer.CoordToOffset(x, y), c);
                            if (eb)
                                buffer.MultPixelAlpha(buffer.CoordToOffset(w - x - 1, y), c);
                        }
                    }
            }
        }

        public BitmapBuffer GetBitmapBuffer()
        {
            return new BitmapBuffer(_bitmap);
        }

        private int PercentVisible(BitmapBuffer buffer)
        {
            var c = 0;
            for (int i = 0; i < buffer.Length; i++)
                if (buffer.GetPixelAlpha(i) >= 255)
                    c++;

            return 100 * c / buffer.Length;
        }

        public bool RemoveBackground(int sensitivity)
        {
            ConvertToFormat(PixelFormat.Format32bppArgb);

            var w = _bitmap.Width - 1;
            var h = _bitmap.Height - 1;

            var colors = new[]
            {
                Bitmap.GetPixel(0, 0), Bitmap.GetPixel(1, 0), Bitmap.GetPixel(0, 1), Bitmap.GetPixel(1, 1),
                Bitmap.GetPixel(w, 0), Bitmap.GetPixel(w-1, 0), Bitmap.GetPixel(w, 1), Bitmap.GetPixel(w-1, 1),
                Bitmap.GetPixel(0, h), Bitmap.GetPixel(1, h), Bitmap.GetPixel(0, h-1), Bitmap.GetPixel(1, h-1),
                Bitmap.GetPixel(w, h), Bitmap.GetPixel(w-1, h), Bitmap.GetPixel(w, h-1), Bitmap.GetPixel(w-1, h-1),
            };

            var candidate = colors
              .GroupBy(x => x)
              .Select(x => new KeyValuePair<Color, int>(x.Key, x.Count()))
              .OrderByDescending(x => x.Value)
              .Select(x => x.Key)
              .ToList();

            if (!candidate.Any())
                return false;

            var color = candidate.First();

            using (var buffer = GetBitmapBuffer())
            {
                for (var pos = 0; pos < buffer.Length; pos++)
                {
                    var similarity = Similarity(Color.FromArgb(buffer.GetPixel(pos)), color);
                    if (similarity < sensitivity)
                        buffer.SetPixelAlpha(pos, (byte)(similarity * (255 / sensitivity)));
                }
            }

            return true;
        }

        public void HighlightColors(ColorMatchDelegate match, Color highlight)
        {
            ConvertToFormat(PixelFormat.Format32bppArgb);

            using (var buffer = GetBitmapBuffer())
            {
                var color = highlight.ToArgb();
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (match(Color.FromArgb(buffer.Buffer[i])))
                        buffer.SetPixel(i, color);
                }
            }
        }

        public void RevealColors(ColorMatchDelegate match)
        {
            ConvertToFormat(PixelFormat.Format32bppArgb);

            using (var buffer = GetBitmapBuffer())
            {
                var colors = new List<int>();
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer.SetPixelAlpha(i, 0);
                    if (match(Color.FromArgb(buffer.Buffer[i])))
                        colors.Add(i);
                }

                while (CompressOffsetList(colors) > 0)
                { }

                var brush = new PaintBrush(2 * Width / 3, 32);
                do
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var pt = buffer.OffsetToCoord(colors[_rnd.Next(colors.Count)]);
                        brush.Reveal(buffer, pt.X + _rnd.Next(-5, 5), pt.Y + _rnd.Next(-5, 5));
                    }
                } while (PercentVisible(buffer) < 10);
            }
        }

        private int CompressOffsetList(List<int> colors)
        {
            var i = 0;
            while (i < colors.Count - 1)
            {
                var c1 = colors[i];
                var c2 = colors[i + 1];

                if (c2 == c1 + 1 || c2 == c1 + 2 || c2 == c1 + 3)
                    colors[i + 1] = -1;

                i++;
            }

            return colors.RemoveAll(c => c < 0);
        }

        public void Resample(int width, int height)
        {
            var newBitmap = CreateNewBitmap(_bitmap, _bitmap.PixelFormat, new Size(width, height));

            using (var g = System.Drawing.Graphics.FromImage(newBitmap))
            {
                var gu = GraphicsUnit.Pixel;
                g.InterpolationMode = InterpolationMode;
                g.DrawImage(_bitmap, newBitmap.GetBounds(ref gu), _bitmap.GetBounds(ref gu), gu);
            }

            _bitmap.Dispose();
            _bitmap = newBitmap;
        }

        public void ScaleMax(Size bounds)
        {
            var newsize = ScaleMax(Size, bounds);
            if (newsize == Size)
                return;

            Resample(newsize.Width, newsize.Height);
        }

        public void ScaleMin(Size bounds)
        {
            var newsize = ScaleMin(Size, bounds);
            if (newsize == Size)
                return;

            Resample(newsize.Width, newsize.Height);
        }

        private int Similarity(Color color1, Color color2)
        {
            return Math.Abs(((color1.R - color2.R) + (color1.G - color2.G) + (color1.B - color2.B)) / 3);
        }

        public static Size ScaleMax(Size original, Size bounds)
        {
            var dx = (double)bounds.Width / original.Width;
            var dy = (double)bounds.Height / original.Height;

            var delta = Math.Min(dx, dy);

            var result = new Size((int)Math.Round(original.Width * delta), (int)Math.Round(original.Height * delta));
            return result;
        }

        public static Size ScaleMin(Size original, Size bounds)
        {
            var dx = (double)bounds.Width / original.Width;
            var dy = (double)bounds.Height / original.Height;

            var delta = Math.Max(dx, dy);

            var result = new Size((int)Math.Round(original.Width * delta), (int)Math.Round(original.Height * delta));
            return result;
        }

        public void Crop(RectangleF rect)
        {
            var bitmap = CreateNewBitmap(_bitmap, _bitmap.PixelFormat, rect.Size.ToSize());
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
                g.DrawImage(_bitmap, 0, 0, rect, GraphicsUnit.Pixel);

            _bitmap.Dispose();
            _bitmap = bitmap;
        }
    }
}

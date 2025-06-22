namespace DotNetCommons.Colors;

// HSL/RGB conversion adapted from Uwe Keim, https://gist.github.com/UweKeim/fb7f829b852c209557bc49c51ba14c8b

internal static class ColorConversion
{
    public static HsbColor GrayscaleToHsb(GrayscaleColor color)
    {
        return new HsbColor(0, 0, color.Value / 2.55, color.Alpha);
    }

    public static HslColor GrayscaleToHsl(GrayscaleColor color)
    {
        return new HslColor(0, 0, color.Value / 2.55, color.Alpha);
    }

    public static RgbColor GrayscaleToRgb(GrayscaleColor color)
    {
        return new RgbColor(color.Value, color.Value, color.Value, color.Alpha);
    }

    public static RgbColor? HexToRgb(string hex)
    {
        if (hex.IsEmpty())
            return null;

        hex = hex.Trim();
        if (hex.StartsWith('#'))
            hex = hex.TrimStart('#');

        if (hex.Length is 3 or 4)
        {
            var r = hex[0];
            var g = hex[1];
            var b = hex[2];
            var a = hex.Length == 4 ? hex[3] : 'F';
            hex = $"{r}{r}{g}{g}{b}{b}{a}{a}";
        }

        if (hex.Length == 6)
            hex += "FF";

        if (hex.Length == 8)
        {
            var value = Convert.ToUInt32(hex, 16);
            return new RgbColor((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
        }

        throw new ArgumentException("Color string must be either 3, 4, 6 or 8 hex characters.");
    }

    public static RgbColor HsbToRgb(HsbColor color)
    {
        double red = 0, green = 0, blue = 0;

        var h = color.Hue;
        var s = color.Saturation / 100;
        var b = color.Brightness / 100;

        if (s < 0.00001)
        {
            b *= 255;
            return new RgbColor(b, b, b, color.Alpha);
        }

        // the color wheel has six sectors.
        var sectorPosition = h / 60;
        var sectorNumber = (int)Math.Floor(sectorPosition);
        var fractionalSector = sectorPosition - sectorNumber;

        var p = b * (1 - s);
        var q = b * (1 - s * fractionalSector);
        var t = b * (1 - s * (1 - fractionalSector));

        // Assign the fractional colors to r, g, and b
        // based on the sector the angle is in.
        switch (sectorNumber)
        {
            case 0:
                red   = b;
                green = t;
                blue  = p;
                break;

            case 1:
                red   = q;
                green = b;
                blue  = p;
                break;

            case 2:
                red   = p;
                green = b;
                blue  = t;
                break;

            case 3:
                red   = p;
                green = q;
                blue  = b;
                break;

            case 4:
                red   = t;
                green = p;
                blue  = b;
                break;

            case 5:
                red   = b;
                green = p;
                blue  = q;
                break;
        }

        return new RgbColor(red * 255, green * 255, blue * 255, color.Alpha);
    }

    public static RgbColor HslToRgb(HslColor color)
    {
        var h = color.Hue / 360.0;
        var s = color.Saturation / 100.0;
        var l = color.Lightness / 100.0;

        if (Math.Abs(s - 0.0) < 0.00001)
        {
            l *= 255;
            return new RgbColor(l, l, l, color.Alpha);
        }

        var var2 = l < 0.5 ? l * (1.0 + s) : l + s - s * l;
        var var1 = 2.0 * l - var2;

        var r = Hue2Rgb(var1, var2, h + 1.0 / 3.0);
        var g = Hue2Rgb(var1, var2, h);
        var b = Hue2Rgb(var1, var2, h - 1.0 / 3.0);

        return new RgbColor(r * 255, g * 255, b * 255, color.Alpha);

        static double Hue2Rgb(double v1, double v2, double vH)
        {
            if (vH < 0.0)
                vH += 1.0;
            if (vH > 1.0)
                vH -= 1.0;

            if (6.0 * vH < 1.0)
                return v1 + (v2 - v1) * 6.0 * vH;

            if (2.0 * vH < 1.0)
                return v2;

            if (3.0 * vH < 2.0)
                return v1 + (v2 - v1) * (2.0 / 3.0 - vH) * 6.0;

            return v1;
        }
    }

    public static string RgbToHex(RgbColor color)
    {
        var (r, g, b, a) = color.GetByteColors();
        return a == 255
            ? $"#{r:X2}{g:X2}{b:X2}"
            : $"#{r:X2}{g:X2}{b:X2}{a:X2}";
    }

    public static GrayscaleColor RgbToGrayscale(RgbColor color)
    {
        return new GrayscaleColor(color.Red * 0.299 + color.Green * 0.587 + color.Blue * 0.114, color.Alpha);
    }

    public static HsbColor RgbToHsb(RgbColor rgb)
    {
        var r = rgb.Red / 255;
        var g = rgb.Green / 255;
        var b = rgb.Blue / 255;

        var min = Math.Min(Math.Min(r, g), b);
        var max = Math.Max(Math.Max(r, g), b);
        var delta = max - min;

        var hue = 0.0;
        var saturation = 0.0;
        var brightness = max * 100;

        if (Math.Abs(max - 0) < 0.00001 || Math.Abs(delta - 0) < 0.00001)
            hue = 0;
        else
        {
            if (Math.Abs(min - 0) < 0.00001)
                saturation = 100;
            else
                saturation = delta / max * 100;

            if (Math.Abs(r - max) < 0.00001)
                hue = (g - b) / delta;
            else if (Math.Abs(g - max) < 0.00001)
                hue = 2 + (b - r) / delta;
            else if (Math.Abs(b - max) < 0.00001)
                hue = 4 + (r - g) / delta;
        }

        hue *= 60;
        if (hue < 0)
            hue += 360;

        return new HsbColor(hue, saturation, brightness, rgb.Alpha);
    }

    public static HslColor RgbToHsl(RgbColor rgb)
    {
        var r = rgb.Red / 255.0;
        var g = rgb.Green / 255.0;
        var b = rgb.Blue / 255.0;

        var min = Math.Min(Math.Min(r, g), b);
        var max = Math.Max(Math.Max(r, g), b);
        var delta = max - min;

        var l = (max + min) / 2;

        // Achromatic?
        if (Math.Abs(delta - 0) < 0.00001)
            return new HslColor(0, 0, l * 100, rgb.Alpha);

        var s = l < 0.5 ? delta / (max + min) : delta / (2.0 - max - min);

        var deltaR = ((max - r) / 6.0 + delta / 2.0) / delta;
        var deltaG = ((max - g) / 6.0 + delta / 2.0) / delta;
        var deltaB = ((max - b) / 6.0 + delta / 2.0) / delta;

        double h;
        if (Math.Abs(r - max) < 0.00001)
            h = deltaB - deltaG;
        else if (Math.Abs(g - max) < 0.00001)
            h = 1.0 / 3.0 + deltaR - deltaB;
        else if (Math.Abs(b - max) < 0.00001)
            h = 2.0 / 3.0 + deltaG - deltaR;
        else
            h = 0.0;

        if (h < 0.0)
            h += 1.0;
        if (h > 1.0)
            h -= 1.0;

        return new HslColor(h * 360.0, s * 100.0, l * 100.0, rgb.Alpha);
    }
}
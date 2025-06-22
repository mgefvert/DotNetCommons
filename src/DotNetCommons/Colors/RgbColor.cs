using System.Drawing;

namespace DotNetCommons.Colors;

public class RgbColor
{
    private double _red;
    private double _green;
    private double _blue;
    private double _alpha;

    public double Red
    {
        get => _red;
        set => _red = Math.Clamp(value, 0, 255);
    }

    public double Green
    {
        get => _green;
        set => _green = Math.Clamp(value, 0, 255);
    }

    public double Blue
    {
        get => _blue;
        set => _blue = Math.Clamp(value, 0, 255);
    }

    public double Alpha
    {
        get => _alpha;
        set => _alpha = Math.Clamp(value, 0, 255);
    }

    public RgbColor()
    {
    }

    public RgbColor(Color color) : this(color.R, color.G, color.B, color.A)
    {
    }

    public RgbColor(double red, double green, double blue, double alpha = 255)
    {
        Red   = red;
        Green = green;
        Blue  = blue;
        Alpha = alpha;
    }

    public RgbColor Clone() => new(Red, Green, Blue, Alpha);

    public RgbColor Darken(double amount) => MixIn(0, 0, 0, amount * 255);

    public RgbColor Lighten(double amount) => MixIn(255, 255, 255, amount * 255);

    public RgbColor MixIn(Color color, double amount = 1) => MixIn(color.R, color.G, color.B, color.A * Math.Clamp(amount, 0, 1));

    public RgbColor MixIn(RgbColor color, double amount = 1) => MixIn(color.Red, color.Green, color.Blue, color.Alpha * Math.Clamp(amount, 0, 1));

    private RgbColor MixIn(double red, double green, double blue, double alpha)
    {
        var r = Math.Clamp(red, 0, 255);
        var g = Math.Clamp(green, 0, 255);
        var b = Math.Clamp(blue, 0, 255);
        var a = Math.Clamp(alpha / 255, 0, 1);

        var invA = 1.0 - a;

        Red   = invA * Red   + r * a;
        Green = invA * Green + g * a;
        Blue  = invA * Blue  + b * a;

        return this;
    }

    public (byte R, byte G, byte B, byte A) GetByteColors()
    {
        return (
            (byte)Math.Round(Red),
            (byte)Math.Round(Green),
            (byte)Math.Round(Blue),
            (byte)Math.Round(Alpha)
        );
    }

    public Color ToColor()
    {
        var (r, g, b, a) = GetByteColors();
        return Color.FromArgb(a, r, g, b);
    }

    public static implicit operator RgbColor(Color color)
    {
        return new RgbColor(color.R, color.G, color.B, color.A);
    }

    public static implicit operator Color(RgbColor color) => color.ToColor();

    public GrayscaleColor ToGrayscale() => ColorConversion.RgbToGrayscale(this);
    
    public string ToHex() => ColorConversion.RgbToHex(this);

    public HsbColor ToHsb() => ColorConversion.RgbToHsb(this);
    
    public HslColor ToHsl() => ColorConversion.RgbToHsl(this);

    public static RgbColor? FromHex(string hex) => ColorConversion.HexToRgb(hex);
}
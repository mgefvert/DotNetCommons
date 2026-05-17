namespace DotNetCommons.Colors;

public class GrayscaleColor
{
    private double _value;
    private double _alpha;

    public double Value
    {
        get => _value;
        set => _value = Clamp(value, 0, 255);
    }

    public double Alpha
    {
        get => _alpha;
        set => _alpha = Clamp(value, 0, 255);
    }

    public GrayscaleColor()
    {
    }

    public GrayscaleColor(double value, double alpha = 255)
    {
        Value = value;
        Alpha = alpha;
    }

    public HsbColor ToHsb() => ColorConversion.GrayscaleToHsb(this);

    public HslColor ToHsl() => ColorConversion.GrayscaleToHsl(this);

    public OklabColor ToOklab() => ColorConversion.GrayscaleToOklab(this);

    public RgbColor ToRgb() => ColorConversion.GrayscaleToRgb(this);

    private static double Clamp(double value, double min, double max)
    {
        return double.IsNaN(value) ? min : Math.Clamp(value, min, max);
    }
}

using System.Drawing;

namespace DotNetCommons.Colors;

public class GrayscaleColor
{
    private double _value;
    private double _alpha;

    public double Value
    {
        get => _value;
        set => _value = Math.Clamp(value, 0, 255);
    }

    public double Alpha
    {
        get => _alpha;
        set => _alpha = Math.Clamp(value, 0, 255);
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
    
    public RgbColor ToRgb() => ColorConversion.GrayscaleToRgb(this);
}
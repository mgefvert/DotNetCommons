namespace DotNetCommons.Colors;

public class HslColor
{
    private double _hue;
    private double _saturation;
    private double _lightness;
    private double _alpha;

    public double Hue
    {
        get => _hue;
        set => _hue = NormalizeHue(value);
    }

    public double Saturation
    {
        get => _saturation;
        set => _saturation = Clamp(value, 0, 100);
    }

    public double Lightness
    {
        get => _lightness;
        set => _lightness = Clamp(value, 0, 100);
    }

    public double Alpha
    {
        get => _alpha;
        set => _alpha = Clamp(value, 0, 255);
    }

    public HslColor()
    {
    }

    public HslColor(double hue, double saturation, double lightness, double alpha = 255)
    {
        Hue = hue;
        Saturation = saturation;
        Lightness = lightness;
        Alpha = alpha;
    }

    public RgbColor ToRgb() => ColorConversion.HslToRgb(this);

    private static double NormalizeHue(double value)
    {
        if (!double.IsFinite(value))
            return 0;

        value %= 360;
        return value < 0 ? value + 360 : value;
    }

    private static double Clamp(double value, double min, double max)
    {
        return double.IsNaN(value) ? min : Math.Clamp(value, min, max);
    }
}

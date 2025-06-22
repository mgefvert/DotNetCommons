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
        set
        {
            while (value < 0) value += 360;
            while (value >= 360) value -= 360;
            _hue = value;
        }
    }

    public double Saturation
    {
        get => _saturation;
        set => _saturation = Math.Clamp(value, 0, 100);
    }

    public double Lightness
    {
        get => _lightness;
        set => _lightness = Math.Clamp(value, 0, 100);
    }

    public double Alpha
    {
        get => _alpha;
        set => _alpha = Math.Clamp(value, 0, 255);
    }

    public HslColor()
    {
    }

    public HslColor(double hue, double saturation, double lightness, double alpha = 255)
    {
        Hue        = hue;
        Saturation = saturation;
        Lightness  = lightness;
        Alpha      = alpha;
    }

    public RgbColor ToRgb() => ColorConversion.HslToRgb(this);
}
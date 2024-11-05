namespace DotNetCommons.Colors;

public class HsbColor
{
    private double _hue;
    private double _saturation;
    private double _brightness;
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

    public double Brightness
    {
        get => _brightness;
        set => _brightness = Math.Clamp(value, 0, 100);
    }

    public double Alpha
    {
        get => _alpha;
        set => _alpha = Math.Clamp(value, 0, 255);
    }

    public HsbColor()
    {
    }

    public HsbColor(double hue, double saturation, double brightness, double alpha = 255)
    {
        Hue        = hue;
        Saturation = saturation;
        Brightness = brightness;
        Alpha      = alpha;
    }

    public RgbColor ToRgb() => ColorConversion.HsbToRgb(this);
}
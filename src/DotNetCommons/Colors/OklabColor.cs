namespace DotNetCommons.Colors;

public class OklabColor
{
    private double _lightness;
    private double _a;
    private double _b;
    private double _alpha;

    public double Lightness
    {
        get => _lightness;
        set => _lightness = Clamp(value, 0, 1);
    }

    public double A
    {
        get => _a;
        set => _a = Normalize(value);
    }

    public double B
    {
        get => _b;
        set => _b = Normalize(value);
    }

    public double Alpha
    {
        get => _alpha;
        set => _alpha = Clamp(value, 0, 255);
    }

    public OklabColor()
    {
    }

    public OklabColor(double lightness, double a, double b, double alpha = 255)
    {
        Lightness = lightness;
        A = a;
        B = b;
        Alpha = alpha;
    }

    public RgbColor ToRgb() => ColorConversion.OklabToRgb(this);

    private static double Clamp(double value, double min, double max)
    {
        return double.IsNaN(value) ? min : Math.Clamp(value, min, max);
    }

    private static double Normalize(double value)
    {
        return double.IsFinite(value) ? value : 0;
    }
}

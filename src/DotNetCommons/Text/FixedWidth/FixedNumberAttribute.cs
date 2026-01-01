using System.Globalization;

namespace DotNetCommons.Text.FixedWidth;

public class FixedNumberAttribute : FixedWidthAttribute
{
    public int Decimals { get; set; }
    public int Scale { get; set; }

    public FixedNumberAttribute(int start, int length) : base(start, length)
    {
    }

    public override string FormatValue(object? value, CultureInfo culture)
    {
        value ??= DefaultValue;
        if (value == null)
            return PadValue("", Alignment.Right);

        var number = Convert.ToDouble(value);
        if (Scale != 0)
            number *= Math.Pow(10, Scale);

        var result = number.ToString("F" + Decimals, culture);
        return PadValue(result, Alignment.Right);
    }

    public override object Parse(string data, CultureInfo culture)
    {
        data = TrimValue(data, Alignment.Right);

        if (string.IsNullOrEmpty(data))
            return DefaultValue ?? 0.0;

        var number = double.Parse(data, NumberStyles.Any, culture);

        if (Scale != 0)
            number /= Math.Pow(10, Scale);

        return number;
    }
}
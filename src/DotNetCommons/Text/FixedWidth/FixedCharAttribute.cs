using System.Globalization;

namespace DotNetCommons.Text.FixedWidth;

public class FixedCharAttribute : FixedWidthAttribute
{
    public FixedCharAttribute(int start) : base(start, 1)
    {
    }

    public override string FormatValue(object? value, CultureInfo culture)
    {
        value ??= DefaultValue;
        if (value == null)
            return Pad.ToString();

        if (value is char c)
            return c == 0 ? Pad.ToString() : c.ToString(); // NUL gets padded with the pad character

        var s = value.ToString() ?? "";
        var result = s.Length switch
        {
            0 => Pad.ToString(),
            1 => s,
            _ => throw new InvalidDataException($"Cannot convert '{s}' to char")
        };

        return PadValue(result, Alignment.Left);
    }

    public override object Parse(string data, CultureInfo culture)
    {
        data = TrimValue(data, Alignment.Left);
        return data.Length == 0 ? Pad : data[0];
    }
}
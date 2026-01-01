using System.Globalization;

namespace DotNetCommons.Text.FixedWidth;

public class FixedDateTimeAttribute : FixedWidthAttribute
{
    public string Format { get; set; } = "yyyyMMdd";

    public FixedDateTimeAttribute(int start, int length) : base(start, length)
    {
    }

    public override string FormatValue(object? value, CultureInfo culture)
    {
        value ??= DefaultValue;
        var result = value switch
        {
            null        => "",
            DateTime dt => dt.ToString(Format, culture),
            DateOnly dt => dt.ToString(Format, culture),
            _           => throw new InvalidDataException($"Cannot convert '{value}' to DateTime")
        };

        return PadValue(result, Alignment.Left);
    }

    public override object Parse(string data, CultureInfo culture)
    {
        data = TrimValue(data, Alignment.Left);
        return DateTime.ParseExact(data, Format, culture);
    }
}
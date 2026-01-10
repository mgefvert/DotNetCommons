using System.Globalization;

namespace DotNetCommons.Text.FixedWidth;

public class FixedBoolAttribute : FixedWidthAttribute
{
    public string True { get; set; } = "Y";
    public string False { get; set; } = "N";

    public FixedBoolAttribute(int start, int length) : base(start, length)
    {
    }

    public override string FormatValue(object? value, CultureInfo culture)
    {
        value ??= DefaultValue;
        var result = value switch
        {
            null   => False,
            bool b => b ? True : False,
            _      => value.ToString().ParseBoolean(false) ? True : False
        };

        return PadValue(result, Alignment.Left);
    }

    // ReSharper disable SimplifyConditionalTernaryExpression
    public override object Parse(string data, CultureInfo culture)
    {
        data = TrimValue(data, Alignment.Left);

        return data == True ? true
            : data == False ? false
            : data.ParseBoolean(false);
    }
    // ReSharper restore SimplifyConditionalTernaryExpression
}
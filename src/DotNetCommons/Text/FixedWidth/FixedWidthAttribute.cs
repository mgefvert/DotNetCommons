using System.Globalization;

namespace DotNetCommons.Text.FixedWidth;

public abstract class FixedWidthAttribute : Attribute
{
    public int Start { get; }

    public int Length { get; }

    public Alignment Align { get; set; } = Alignment.Default;

    public object? DefaultValue { get; set; }

    public char Pad { get; set; } = ' ';

    protected FixedWidthAttribute(int start, int length)
    {
        if (start <= 0)
            throw new ArgumentOutOfRangeException(nameof(start), "Start must be greater than zero (offsets are 1-based).");

        Start  = start;
        Length = length;
    }

    public abstract string FormatValue(object? value, CultureInfo culture);
    public abstract object? Parse(string data, CultureInfo culture);

    protected string PadValue(string value, Alignment defaultAlign)
    {
        if (value.Length > Length)
            throw new InvalidDataException($"Length of formatted value '{value}' exceeds the maximum fields length of {Length}.");

        if (value.Length == Length)
            return value;

        var align = Align;
        if (align == Alignment.Default)
            align = defaultAlign;

        return align switch
        {
            Alignment.Left  => value.PadRight(Length, Pad),
            Alignment.Right => value.PadLeft(Length, Pad),
            _               => throw new InvalidOperationException("Alignment value must be Left/Right in PadValue")
        };
    }

    protected string TrimValue(string value, Alignment defaultAlign)
    {
        var align = Align;
        if (align == Alignment.Default)
            align = defaultAlign;

        return align switch
        {
            Alignment.Left  => value.TrimEnd(Pad),
            Alignment.Right => value.TrimStart(Pad),
            _               => throw new InvalidOperationException("Alignment value must be Left/Right in TrimValue")
        };
    }

}
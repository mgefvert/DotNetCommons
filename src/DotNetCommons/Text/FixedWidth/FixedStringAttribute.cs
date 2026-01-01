using System.Globalization;
using System.Text;

namespace DotNetCommons.Text.FixedWidth;

public class FixedStringAttribute : FixedWidthAttribute
{
    private HashSet<char>? _allowedChars;

    public string? AllowedChars { get; set; }
    public string? Format { get; set; }
    public bool UpperCase { get; set; }

    public FixedStringAttribute(int start, int length) : base(start, length)
    {
    }

    public override string FormatValue(object? value, CultureInfo culture)
    {
        value ??= DefaultValue;
        var result = value?.ToString() ?? "";
        if (UpperCase)
            result = result.ToUpper();

        if (AllowedChars != null)
        {
            _allowedChars ??= new HashSet<char>(AllowedChars);

            var sb = new StringBuilder(result.Length);
            foreach (var c in result.Where(c => _allowedChars.Contains(c)))
                sb.Append(c);

            result = sb.ToString();
        }

        return PadValue(result, Alignment.Left);
    }

    public override object Parse(string data, CultureInfo culture)
    {
        return TrimValue(data, Alignment.Left);
    }
}
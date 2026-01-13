using System.Text;

namespace DotNetCommons.IO;

public enum ProgressBarType
{
    Bar,
    Arrow,
    Braille
}

public class ProgressBar
{
    private readonly ProgressBarType _type;
    private readonly int _width;
    private readonly bool _drawPercentCenter;
    private readonly StringBuilder _buffer;

    private string? _last;

    public ProgressBar(ProgressBarType type, int width, bool drawPercentCenter)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 8);

        _type              = type;
        _width             = width;
        _drawPercentCenter = drawPercentCenter;
        _buffer            = new StringBuilder(width);
    }

    public void Clear()
    {
        Console.CursorVisible = false;
        Console.Write("\r");
        Console.Write(new string(' ', _width + 2)); // +2 for potential brackets
        Console.Write("\r");
        Console.CursorVisible = true;
    }

    public void Draw(double progress)
    {
        var s = Render(progress);
        if (s == _last)
            return;

        Console.CursorVisible = false;
        Console.Write("\r");
        Console.Write(s);
        Console.Write("\r");
        Console.CursorVisible = true;

        _last = s;
    }

    public void DrawLine(double progress)
    {
        Console.WriteLine(Render(progress));
    }

    public string Render(double completion)
    {
        _buffer.Clear();
        completion = Math.Round(Math.Clamp(completion, 0.0, 1.0), 4, MidpointRounding.AwayFromZero);

        var area      = _width - 2;
        var chars     = completion * area;
        var full      = (int)Math.Truncate(chars);
        var building  = area == full ? -1 : chars - Math.Truncate(chars);
        var remaining = area == full ? 0 : _width - full - 3;

        _buffer.Append('[');
        switch (_type)
        {
            case ProgressBarType.Arrow:
                RenderArrow(full, building, remaining);
                break;

            case ProgressBarType.Bar:
                RenderBar(full, building, remaining);
                break;

            case ProgressBarType.Braille:
                RenderBraille(full, building, remaining);
                break;
        };
        _buffer.Append(']');

        if (_drawPercentCenter)
        {
            var pct      = Math.Round(completion, 2, MidpointRounding.AwayFromZero).ToString("P0");
            var startPos = (_buffer.Length - pct.Length) / 2;

            for (var i = 0; i < pct.Length; i++)
                _buffer[startPos + i] = pct[i];
        }

        return _buffer.ToString();
    }

    private void RenderArrow(int full, double building, int remaining)
    {
        if (full > 0)
            _buffer.Append('=', full);
        if (building >= 0)
            _buffer.Append('>');
        if (remaining > 0)
            _buffer.Append(' ', remaining);
    }

    private void RenderBar(int full, double building, int remaining)
    {
        if (full + 1 > 0)
            _buffer.Append('#', full + 1);
        if (remaining > 0)
            _buffer.Append('-', remaining);
    }

    // Braille pattern
    private static readonly char[] BrailleSeries = [
        ' ', '\u2840', '\u28c0', '\u28c4', '\u28e4', '\u28e6', '\u28f6', '\u28f7', '\u28ff'
    ];

    private void RenderBraille(int full, double building, int remaining)
    {
        if (full > 0)
            _buffer.Append(BrailleSeries[^1], full);
        if (building >= 0)
            _buffer.Append(BrailleSeries[(int)(building * BrailleSeries.Length)]);
        if (remaining > 0)
            _buffer.Append(BrailleSeries[0], remaining);
    }
}
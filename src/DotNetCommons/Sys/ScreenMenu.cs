namespace DotNetCommons.Sys;

public record ScreenMenuResult<T>(T Item, ConsoleKey Key);

public interface IScreenMenuState
{
    ConsoleKey Key { get; }
    string Name { get; }
    void ExecuteAction();
    string ToString();
}

public class ScreenMenuState<T>(ConsoleKey key, string name, T? value, Func<T?, T?> action) : IScreenMenuState
{
    public ConsoleKey Key { get; } = key;
    public string Name { get; } = name;
    public T? Value { get; set; } = value;
    public Func<T?, T?> Action { get; } = action;

    public void ExecuteAction() => Value = Action(Value);
    public override string ToString() => $"{Name}={Value}";
}

public class ScreenMenu<T>
{
    private record ScreenMenuColumn(string Header, Func<T, string> Value);

    private const int ColumnPadding = 2;

    private readonly List<ScreenMenuColumn> _columns = [];
    private int[] _columnWidths = null!;
    private IReadOnlyList<T> _items = null!;

    public Dictionary<ConsoleKey, string> ExitActions { get; } = new();
    public List<IScreenMenuState> State { get; } = new();
    public string Footer { get; set; } = "[Enter]=Accept [Esc]=Exit";

    public ScreenMenu<T> AddColumn(string header, Func<T, string> value)
    {
        _columns.Add(new ScreenMenuColumn(header, value));
        return this;
    }

    public ScreenMenu<T> AddExitAction(ConsoleKey key, string name)
    {
        ExitActions[key] = name;
        return this;
    }

    public ScreenMenu<T> AddState(IScreenMenuState state)
    {
        State.Add(state);
        return this;
    }

    public ScreenMenuResult<T>? Show(IReadOnlyList<T> items)
    {
        if (_columns.Count == 0)
            throw new InvalidOperationException("ScreenMenu requires at least one column.");

        _items        = items;
        _columnWidths = CalculateColumnWidths();

        var selected = 0;
        var top = 0;
        var width = 0;
        var height = 0;
        var fullRedraw = true;

        Console.CursorVisible = false;
        try
        {
            Console.Clear();

            for (;;)
            {
                var visibleRows = Math.Max(1, Console.WindowHeight - 3);
                if (width != Console.WindowWidth || height != Console.WindowHeight)
                {
                    width      = Console.WindowWidth;
                    height     = Console.WindowHeight;
                    fullRedraw = true;
                    top        = ClampTop(selected, top, visibleRows);
                }

                if (fullRedraw)
                {
                    DrawScreen(selected, top, visibleRows);
                    fullRedraw = false;
                }

                var key = Console.ReadKey(true);
                var previousSelected = selected;
                var previousTop = top;

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        selected = Math.Max(0, selected - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selected = Math.Min(_items.Count - 1, selected + 1);
                        break;
                    case ConsoleKey.PageUp:
                        selected = Math.Max(0, selected - visibleRows);
                        break;
                    case ConsoleKey.PageDown:
                        selected = Math.Min(_items.Count - 1, selected + visibleRows);
                        break;
                    case ConsoleKey.Enter:
                        return new ScreenMenuResult<T>(_items[selected], key.Key);
                    case ConsoleKey.Escape:
                        return null;
                    default:
                        if (ExitActions.ContainsKey(key.Key))
                            return new ScreenMenuResult<T>(_items[selected], key.Key);

                        var stateAction = State.FirstOrDefault(x => x.Key == key.Key);
                        stateAction?.ExecuteAction();
                        fullRedraw = true;
                        break;
                }

                top = ClampTop(selected, top, visibleRows);

                if (top != previousTop)
                {
                    DrawScreen(selected, top, visibleRows);
                }
                else if (selected != previousSelected)
                {
                    DrawItemRow(previousSelected, top, visibleRows, false);
                    DrawItemRow(selected, top, visibleRows, true);
                }
            }
        }
        finally
        {
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.Clear();
        }
    }

    private int[] CalculateColumnWidths()
    {
        var widths = new int[_columns.Count];
        for (var i = 0; i < _columns.Count; i++)
        {
            widths[i] = Math.Max(
                _columns[i].Header.Length,
                _items.Max(item => _columns[i].Value(item).Length));
        }

        return widths;
    }

    private int ClampTop(int selected, int top, int visibleRows)
    {
        if (selected < top)
            return selected;
        if (selected >= top + visibleRows)
            return selected - visibleRows + 1;
        if (top + visibleRows > _items.Count)
            return Math.Max(0, _items.Count - visibleRows);
        return top;
    }

    private void DrawScreen(int selected, int top, int visibleRows)
    {
        Console.ResetColor();
        DrawHeader();
        DrawSeparator();

        for (var row = 0; row < visibleRows; row++)
        {
            var itemIndex = top + row;
            if (itemIndex < _items.Count)
                DrawItemRow(itemIndex, top, visibleRows, itemIndex == selected);
            else
                ClearLine(row + 2);
        }

        DrawHelp(top, visibleRows);
    }

    private void DrawHeader()
    {
        SetNormalColors();
        ClearLine(0);

        var pos = 0;
        for (var i = 0; i < _columns.Count; i++)
        {
            WriteAt(pos, 0, _columns[i].Header, _columnWidths[i]);
            pos += _columnWidths[i] + ColumnPadding;
        }
    }

    private void DrawSeparator()
    {
        SetNormalColors();
        Console.SetCursorPosition(0, 1);
        Console.Write(new string('-', WritableWidth));
    }

    private void DrawItemRow(int itemIndex, int top, int visibleRows, bool selected)
    {
        var row = itemIndex - top + 2;
        if (row < 2 || row >= visibleRows + 2)
            return;

        if (selected)
            SetSelectedColors();
        else
            SetNormalColors();

        ClearLine(row);

        var item = _items[itemIndex];
        var pos = 0;
        for (var i = 0; i < _columns.Count; i++)
        {
            WriteAt(pos, row, _columns[i].Value(item), _columnWidths[i]);
            pos += _columnWidths[i] + ColumnPadding;
        }

        Console.ResetColor();
    }

    private void DrawHelp(int top, int visibleRows)
    {
        SetNormalColors();
        var lastVisible = Math.Min(_items.Count, top + visibleRows);

        var leftText = Footer;
        foreach (var x in ExitActions)
            leftText += $", {x.Value}";

        var rightText = $" ({top + 1}-{lastVisible} of {_items.Count})";
        foreach (var x in State)
            rightText = $"{x.ToString()} " + rightText;

        var help = leftText.PadRight(WritableWidth - rightText.Length - 1) + rightText;
        WriteAt(0, Console.WindowHeight - 1, help, WritableWidth);
    }

    private void ClearLine(int row)
    {
        if (row < 0 || row >= Console.WindowHeight)
            return;

        Console.SetCursorPosition(0, row);
        Console.Write(new string(' ', WritableWidth));
    }

    private static void WriteAt(int left, int top, string text, int width)
    {
        if (left >= WritableWidth || top >= Console.WindowHeight)
            return;

        var available = Math.Min(width, WritableWidth - left);
        Console.SetCursorPosition(left, top);
        Console.Write(Fit(text, available).PadRight(available));
    }

    private static int WritableWidth => Math.Max(0, Console.WindowWidth - 1);

    private static string Fit(string text, int width)
    {
        if (width <= 0)
            return "";
        if (text.Length <= width)
            return text;
        if (width == 1)
            return text[..1];
        return text[..(width - 1)] + ">";
    }

    private static void SetNormalColors()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    private static void SetSelectedColors()
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
    }
}
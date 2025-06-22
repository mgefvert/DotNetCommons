namespace DotNetCommons.Temporal;

public class DebugTimer
{
    public string Name { get; }
    private DateTime _start;
    private DateTime _last;

    public DebugTimer(string name)
    {
        Name = name;
        Start();
    }

    private void Start()
    {
        _start = DateTime.Now;
        _last = DateTime.Now;
    }

    public TimeSpan Lap()
    {
        var now = DateTime.Now;
        var lap = now - _last;
        _last = now;
        return lap;
    }

    public TimeSpan Total()
    {
        return DateTime.Now - _start;
    }

    public void WriteLap(string text)
    {
        Console.WriteLine($"# Timer ({Name}): lap={Lap()}, total={Total()} for {text}");
    }

    public void WriteTotal(string text)
    {
        Console.WriteLine($"# Timer ({Name}): total={Lap()} for {text}");
    }
}
namespace DotNetCommons.Temporal;

// ReSharper disable once UnusedType.Global
public class TestClock : IClock
{
    public DateTime Value { get; set; }

    public DateTime Now => Value;
    public DateTime UtcNow => Value.ToUniversalTime();
    public DateTime Today => Value.Date;
    public DateOnly TodayDate => DateOnly.FromDateTime(Value);

    public TestClock()
    {
        Value = DateTime.Now;
    }

    public TestClock(int year, int month, int day)
    {
        Value = new DateTime(year, month, day);
    }

    public TestClock(int year, int month, int day, int hour, int minute, int second, int millis = 0)
    {
        Value = new DateTime(year, month, day, hour, minute, second, millis);
    }

    public DateTime Advance(TimeSpan time)
    {
        return Value = Value.Add(time);
    }
}
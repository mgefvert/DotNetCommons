namespace DotNetCommons.Temporal;

public class FakeTimeProvider : TimeProvider
{
    public DateTime Value { get; set; }

    public FakeTimeProvider()
    {
        Value = DateTime.Now;
    }

    public FakeTimeProvider(DateTime now)
    {
        Value = now;
    }

    public FakeTimeProvider(int y, int m, int d)
    {
        Value = new DateTime(y, m, d, 0, 0, 0);
    }

    public FakeTimeProvider(int y, int m, int d, int h, int mm, int s)
    {
        Value = new DateTime(y, m, d, h, mm, s);
    }

    public override DateTimeOffset GetUtcNow()
    {
        return Value.ToUniversalTime();
    }

    public FakeTimeProvider Advance(TimeSpan amount)
    {
        Value = Value.Add(amount);
        return this;
    }

    public FakeTimeProvider AdvanceSecond(int seconds = 1)
    {
        Value = Value.AddSeconds(seconds);
        return this;
    }

    public FakeTimeProvider AdvanceMinute(int minutes = 1)
    {
        Value = Value.AddMinutes(minutes);
        return this;
    }

    public FakeTimeProvider AdvanceHour(int hours = 1)
    {
        Value = Value.AddHours(hours);
        return this;
    }

    public FakeTimeProvider AdvanceDay(int days = 1)
    {
        Value = Value.AddDays(days);
        return this;
    }

    public FakeTimeProvider AdvanceMonth(int months = 1)
    {
        Value = Value.AddMonths(months);
        return this;
    }

    public FakeTimeProvider AdvanceYear(int years = 1)
    {
        Value = Value.AddYears(years);
        return this;
    }
}
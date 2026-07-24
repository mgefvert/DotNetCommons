namespace DotNetCommons;

public static class CommonTimeProviderExtensions
{
    extension(TimeProvider clock)
    {
        public DateTime Now => clock.GetLocalNow().DateTime;
        public TimeSpan TimeOfDay => clock.GetLocalNow().TimeOfDay;
        public DateTime Today => clock.GetLocalNow().Date;
        public DateTime Yesterday => clock.GetLocalNow().Date.AddDays(-1);
        public DateTime Tomorrow => clock.GetLocalNow().Date.AddDays(1);
        public DateTime UtcNow => clock.GetUtcNow().DateTime;
        public DayOfWeek DayOfWeek => clock.GetUtcNow().DateTime.DayOfWeek;
    }
}
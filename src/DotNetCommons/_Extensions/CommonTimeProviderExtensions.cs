namespace DotNetCommons;

public static class CommonTimeProviderExtensions
{
    public static DateTime Now(this TimeProvider clock) => clock.GetLocalNow().DateTime;
    public static DateTime UtcNow(this TimeProvider clock) => clock.GetUtcNow().DateTime;
    public static DateTime Today(this TimeProvider clock) => clock.GetLocalNow().Date;
}
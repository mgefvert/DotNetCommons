namespace DotNetCommons.Sys;

public interface IClockJob
{
    public Task Run(JobContext context);
}
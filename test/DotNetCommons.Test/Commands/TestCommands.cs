using DotNetCommons.Commands;
using DotNetCommons.Sys;

namespace DotNetCommons.Test.Commands;

[CommandAction(["command", "one"], "Command one", [])]
public class CommandOne : CommandAction
{
    private readonly TestReporter _reporter;

    public CommandOne(TestReporter reporter)
    {
        _reporter = reporter;
    }

    public override int Execute()
    {
        _reporter.Add("CommandOne");
        return 0;
    }
}

public class ReturnValueArgs
{
    [CommandLineOption('v', "value")]
    public int ReturnValue { get; set; }

    public ReturnValueArgs()
    {
    }

    public ReturnValueArgs(int returnValue)
    {
        ReturnValue = returnValue;
    }
}

[CommandAction(["command", "two"], "Command two", [])]
public class CommandTwo : CommandAction<ReturnValueArgs>
{
    private readonly TestReporter _reporter;

    public CommandTwo(TestReporter reporter)
    {
        _reporter = reporter;
    }

    public override int Execute()
    {
        _reporter.Add($"CommandTwo:{Args.ReturnValue}");
        return Args.ReturnValue;
    }
}

[CommandAction(["test"], "Test", [])]
public class CommandTest : CommandAction
{
    private readonly TestReporter _reporter;

    public CommandTest(TestReporter reporter)
    {
        _reporter = reporter;
    }

    public override int Execute()
    {
        _reporter.Add("CommandTest");

        Registry.TrySchedule<CommandOne>(80, false);
        Registry.TrySchedule<CommandTwo, ReturnValueArgs>(90, false, new ReturnValueArgs(0));

        return 0;
    }
}


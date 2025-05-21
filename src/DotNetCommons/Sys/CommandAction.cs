namespace DotNetCommons.Sys;

public interface ICommandAction
{
    int Execute();
}

public abstract class CommandAction<TArgs> : ICommandAction
    where TArgs : class, new()
{
    public TArgs Args { get; set; }

    public abstract int Execute();
}

public abstract class CommandAction : CommandAction<object>
{
    public CommandAction()
    {
        Args = new object();
    }
}

using System.Reflection;

namespace DotNetCommons.Sys;

public class CommandActionException : Exception
{
    public CommandActionException(string message) : base(message)
    {
    }
}

public class CommandActionInitializationException : Exception
{
    public CommandActionInitializationException(Type type, string reason)
        : base($"Type {type.Name} failed to inialize: {reason}")
    {
    }
}

public class CommandActionResolveException : Exception
{
    public CommandActionResolveException(string message) : base(message)
    {
    }
}

public class CommandActionNoCommandFoundException : Exception
{
    public CommandActionNoCommandFoundException(string message) : base(message)
    {
    }
}

public class CommandActionMultipleCommandsFoundException : Exception
{
    public Type[] Commands { get; }

    public CommandActionMultipleCommandsFoundException(Type[] commands)
    {
        Commands = commands;
    }
}

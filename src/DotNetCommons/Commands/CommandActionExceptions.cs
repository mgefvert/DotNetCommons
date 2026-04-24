namespace DotNetCommons.Commands;

public class CommandActionException(string message, Exception? innerException = null) : Exception(message, innerException);

public class CommandActionInitializationException(Type type, string reason) : Exception($"Type {type.Name} failed to inialize: {reason}");

public class CommandActionResolveException(string message) : Exception(message);

public class CommandActionNoCommandFoundException(string message) : Exception(message);

public class CommandActionMultipleCommandsFoundException(Type[] commands) : Exception
{
    public Type[] Commands { get; } = commands;
}

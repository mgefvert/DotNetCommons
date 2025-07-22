using System.Collections.Concurrent;
using System.Reflection;
using DotNetCommons.Text;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCommons.Commands;

/// <summary>
/// Provides a registry to manage and execute command actions with defined priorities and execution strategies.
/// </summary>
public class CommandActionRegistry
{
    /// <summary>Represents the exit code indicating successful execution of a command.</summary>
    public const int ExitCodeSuccess = 0;

    /// <summary>Represents the exit code indicating that no jobs were scheduled for execution.</summary>
    public const int ExitCodeNoJobsScheduled = -1;

    /// <summary>Represents the exit code indicating a fatal error occurred during the execution of a command.</summary>
    public const int ExitCodeFatalError = -2;

    /// <summary>Specifies the priority level for the first command scheduled with the Execute method.</summary>
    public const int FirstPriority = 10;

    /// <summary>Represents a high-priority level for scheduling command actions.</summary>
    public const int HighPriority = 30;

    /// <summary>Represents a medium-priority level for scheduling command actions.</summary>
    public const int MediumPriority = 50;

    /// <summary>Represents a low-priority level for scheduling command actions.</summary>
    public const int LowPriority = 70;

    /// <summary>Represents the priority for commands that are supposed to run absolutely last in the queue.</summary>
    public const int LastPriority = 99;

    private Action<CommandAction, int>? _afterActionCallback;
    private Action<CommandAction>? _beforeActionCallback;
    private readonly ConcurrentDictionary<string, Type> _commandRegistry = [];
    private readonly IServiceProvider _serviceProvider;
    private readonly ICommandLineParser _commandLineParser;
    private readonly PriorityQueue<Invocation, int> _invocationQueue = new();

    /// <summary>
    /// Provides a registry for managing the registration, scheduling, and execution of command actions.
    /// Uses the DotNetCommons command line parser to parse arguments on the command line.
    /// </summary>
    public CommandActionRegistry(IServiceProvider serviceProvider)
        : this(serviceProvider, new DotNetCommonsCommandLineParser())
    {
    }

    /// <summary>
    /// Provides a registry for managing the registration, scheduling, and execution of command actions.
    /// Uses a custom command line parser to parse arguments on the command line.
    /// </summary>
    public CommandActionRegistry(IServiceProvider serviceProvider, ICommandLineParser commandLineParser)
    {
        _serviceProvider   = serviceProvider;
        _commandLineParser = commandLineParser;
    }

    /// <summary>
    /// Sets a callback to be executed after a command action is performed, providing information about the action and its result status.
    /// </summary>
    /// <param name="action">The callback function to be executed after a command action.</param>
    public CommandActionRegistry AfterAction(Action<CommandAction, int> action)
    {
        _afterActionCallback = action;
        return this;
    }

    /// <summary>
    /// Sets a callback to be executed before a command action is performed, providing information about the action.
    /// </summary>
    /// <param name="action">The callback function to be executed before a command action.</param>
    public CommandActionRegistry BeforeAction(Action<CommandAction> action)
    {
        _beforeActionCallback = action;
        return this;
    }

    /// <summary>
    /// Displays detailed help information for the specified command type.
    /// </summary>
    /// <param name="command">The type of the command for which help information is to be displayed.</param>
    private void DisplayHelp(Type command)
    {
        var attr       = command.GetCustomAttribute<CommandActionAttribute>()!;
        var optionType = GetActionOptionType(command);
        var route      = GetRouteName(attr.Route);

        Console.WriteLine(route);
        Console.WriteLine(new string('=', route.Length));

        if (optionType != null)
        {
            Console.WriteLine();
            _commandLineParser.DisplayHelpFor(Console.Out, optionType);
        }

        if (attr.HelpText.IsAtLeastOne())
        {
            foreach (var (paragraph, index) in attr.HelpText.WithIndex())
            foreach (var line in TextTools.WordWrap(paragraph, Console.WindowWidth, index == 0 ? 0 : -2))
                Console.WriteLine(line);
        }
    }

    /// <summary>
    /// Displays the help information for the provided commands, including their routes and descriptions.
    /// </summary>
    /// <param name="commands">An array of command types.</param>
    private void DisplayHelp(Type[] commands)
    {
        var entries = commands.Select(x => x.GetCustomAttribute<CommandActionAttribute>()).ToDictionary(
            x => GetRouteName(x!.Route),
            x => x!.Description
        );
        var maxRoute = entries.Keys.Max(x => x.Length);

        foreach (var entry in entries.OrderBy(x => x.Key))
        {
            Console.WriteLine($"{entry.Key.PadRight(maxRoute)}  {entry.Value}");
        }
    }

    /// <summary>
    /// Executes the registered command action based on the provided array of arguments.
    /// </summary>
    /// <param name="args">An array of strings representing the command-line arguments passed to the application.</param>
    /// <returns>
    /// An integer representing the exit code of the executed command.
    /// Typically, 0 indicates success, while non-zero values indicate errors or specific execution outcomes.
    /// </returns>
    public int Execute(string[] args)
    {
        if (_commandRegistry.IsEmpty())
            throw new CommandActionResolveException("No command actions are defined.");

        var route = Resolve(args);

        if (route.help)
        {
            if (route.commands.IsEmpty())
                route.commands = _commandRegistry.Values.ToArray();

            if (route.commands.IsOne())
                DisplayHelp(route.commands.Single());
            else
                DisplayHelp(route.commands);

            return 1;
        }

        if (route.commands.Length == 0)
            throw new CommandActionNoCommandFoundException("No such command found.");

        if (route.commands.Length > 1)
        {
            DisplayHelp(route.commands);
            return 1;
        }

        Schedule(FirstPriority, true, route.commands.Single(), route.remaining);

        return ExecuteScheduler();
    }

    /// <summary>
    /// Executes a command action based on the provided invocation, handling pre-action and post-action callbacks.
    /// </summary>
    private int ExecuteCommand(Invocation invocation)
    {
        using var scope = _serviceProvider.CreateScope();

        // Try-catch block for initialization exceptions
        CommandAction command;
        try
        {
            // Create the command objects and initialize it
            command = (CommandAction)ActivatorUtilities.CreateInstance(scope.ServiceProvider, invocation.Action);
            command.Registry = this;

            // Set the optional argument object
            var prop = invocation.Action.GetProperty(nameof(CommandAction<object>.Args));
            prop?.SetValue(command, invocation.Options);

            // Call the before action handler
            _beforeActionCallback?.Invoke(command);
        }
        catch (Exception e)
        {
            throw new CommandActionException($"{invocation.Action.Name}: {e.Message}", e);
        }

        // Try-catch block for anything that requires the after action handler to be called
        try
        {
            var result = command.Execute();
            _afterActionCallback?.Invoke(command, result);

            return result;
        }
        catch (Exception e)
        {
            _afterActionCallback?.Invoke(command, ExitCodeFatalError);
            throw new CommandActionException($"{invocation.Action.Name}: {e.Message}", e);
        }
    }

    /// <summary>
    /// Executes the scheduled commands from the invocation queue in priority order.
    /// If a command fails and its "ContinueOnError" property is set to false, execution halts.
    /// Returns the result of the last executed command or a designated exit code if no jobs are executed.
    /// </summary>
    /// <returns>
    /// The result code of the last executed command, or -1 if no commands were scheduled.
    /// </returns>
    public int ExecuteScheduler()
    {
        var result = ExitCodeNoJobsScheduled;
        while (_invocationQueue.Count > 0)
        {
            var invocation = _invocationQueue.Dequeue();
            result = ExecuteCommand(invocation);

            if (result != 0 && !invocation.ContinueOnError)
                break;
        }

        return result;
    }

    /// <summary>
    /// Retrieves the type of the options associated with a given command type.
    /// </summary>
    /// <param name="commandType">The <see cref="Type"/> of the command whose associated options type is to be resolved. This must inherit
    /// from a generic CommandAction&lt;&gt; type.</param>
    private static Type? GetActionOptionType(Type commandType)
    {
        Type? commandActionClass = commandType;
        while (commandActionClass != null)
        {
            if (commandActionClass.IsGenericType && commandActionClass.GetGenericTypeDefinition() == typeof(CommandAction<>))
                return commandActionClass.GetGenericArguments().Single();

            commandActionClass = commandActionClass.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Constructs a route name by joining the elements of the provided collection, e.g. ["set", "value"] => "set value".
    /// </summary>
    private static string GetRouteName(ICollection<string> route)
    {
        return string.Join(' ', route
            .Where(r => r.IsSet())
            .Select(r => r.ToLower()));
    }

    /// <summary>
    /// Constructs a route path by joining the elements of the provided collection with pipes, e.g. ["set", "value"] => "|set|value|".
    /// </summary>
    private static string GetRoutePath(ICollection<string> route)
    {
        var result = '|' + string.Join('|', route
                             .Where(r => r.IsSet())
                             .Select(r => r.ToLower()))
                         + '|';

        while (result.Contains("||"))
            result = result.Replace("||", "|");

        return result;
    }

    /// <summary>
    /// Checks if a command of the specified type is scheduled in the invocation queue.
    /// </summary>
    public bool IsCommandScheduled(Type type)
    {
        return _invocationQueue.UnorderedItems.Any(x => x.Element.Action == type);
    }

    /// <summary>
    /// Checks if a command of the specified type is scheduled in the invocation queue.
    /// </summary>
    public bool IsCommandScheduled<TCommand>()
        where TCommand : CommandAction
    {
        return _invocationQueue.UnorderedItems.Any(x => x.Element.Action == typeof(TCommand));
    }

    /// <summary>
    /// Resolves the command actions based on the provided arguments, determines if help is requested,
    /// and returns the matching command types along with any remaining arguments.
    /// </summary>
    /// <param name="args">An array of arguments provided for command resolution.</param>
    public (Type[] commands, bool help, string[] remaining) Resolve(string[] args)
    {
        if (_commandRegistry.IsEmpty())
            throw new CommandActionResolveException("No command actions are defined.");

        var route     = args.TakeWhile(x => !x.StartsWith('/') && !x.StartsWith('-')).ToList();
        var remaining = args.Skip(route.Count).ToArray();
        var help      = false;

        if (route.IsAtLeastOne() && route.First() == "help")
        {
            route.ExtractFirst();
            help = true;
        }

        if (route.IsAtLeastOne() && route.Last() == "help")
        {
            route.ExtractLast();
            help = true;
        }

        var search = GetRoutePath(route);
        var types = _commandRegistry
            .Where(x => x.Key.StartsWith(search, StringComparison.CurrentCultureIgnoreCase))
            .Select(x => x.Value)
            .ToArray();

        return (types, help, remaining);
    }

    /// <summary>
    /// Registers the specified assemblies by scanning for classes that implement CommandAction and are decorated with the
    /// CommandActionAttribute. These identified command actions are added to the command registry for execution.
    /// </summary>
    /// <param name="assemblies">An array of assemblies to scan for command action classes to register.</param>
    public CommandActionRegistry RegisterAssemblies(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var commandClasses = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsAssignableTo(typeof(CommandAction)))
                .ToArray();

            foreach (var commandClass in commandClasses)
            {
                var attr = commandClass.GetCustomAttribute<CommandActionAttribute>();
                if (attr == null)
                    throw new CommandActionInitializationException(commandClass,
                        "CommandAction class must have a [CommandAction] attribute.");

                var route = GetRoutePath(attr.Route);
                if (route.IsEmpty())
                    throw new CommandActionInitializationException(commandClass, "[CommandAction] attribute must provide a valid route.");

                if (!_commandRegistry.TryAdd(route, commandClass))
                    throw new CommandActionInitializationException(commandClass, "A command with the same route is already defined.");
            }
        }

        return this;
    }

    /// <summary>
    /// Registers all command actions from the calling assembly by scanning for classes that implement CommandAction and are decorated
    /// with the CommandActionAttribute. The identified command actions are automatically added to the command registry.
    /// </summary>
    public CommandActionRegistry RegisterThis()
    {
        return RegisterAssemblies(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Schedules a command action for execution with the specified priority, error handling behavior, command type,
    /// and command-line arguments.
    /// </summary>
    /// <param name="priority">The priority level of the command action in the execution queue. Lower values indicate earlier execution
    /// in the priority queue.</param>
    /// <param name="continueOnError">Indicates whether subsequent actions should continue executing if an error occurs</param>
    /// <param name="type">The command action to execute.</param>
    /// <param name="args">Optional arguments that the command action requires.</param>
    public CommandActionRegistry Schedule(int priority, bool continueOnError, Type type, string[] args)
    {
        if (!type.IsAssignableTo(typeof(CommandAction)))
            throw new CommandActionInitializationException(type, "Type must inherit from CommandAction.");

        var argsType = GetActionOptionType(type);
        if (argsType == null)
            _invocationQueue.Enqueue(new Invocation(type, null, continueOnError), priority);
        else
        {
            var options = _commandLineParser.Parse(argsType, args);
            _invocationQueue.Enqueue(new Invocation(type, options, continueOnError), priority);
        }

        return this;
    }

    /// <summary>
    /// Schedules a command action for execution with the specified priority and error handling behavior. The command is supposed to be
    /// a CommandAction that does not accept an argument object.
    /// </summary>
    /// <param name="priority">The priority level of the command action in the execution queue. Lower values indicate earlier execution
    /// in the priority queue.</param>
    /// <param name="continueOnError">Indicates whether subsequent actions should continue executing if an error occurs</param>
    public CommandActionRegistry Schedule<TCommand>(int priority, bool continueOnError)
        where TCommand : CommandAction
    {
        var argsType = GetActionOptionType(typeof(TCommand));
        if (argsType != null)
            throw new CommandActionException($"Command action {typeof(TCommand).Name} needs an argument object of type {argsType.Name}.");

        _invocationQueue.Enqueue(new Invocation(typeof(TCommand), null, continueOnError), priority);
        return this;
    }

    /// <summary>
    /// Schedules a command action for execution with the specified priority and error handling behavior. The command is supposed to be
    /// a CommandAction&lt;TArgs&gt; that does requires a specific argument object.
    /// </summary>
    /// <param name="priority">The priority level of the command action in the execution queue. Lower values indicate earlier execution
    /// in the priority queue.</param>
    /// <param name="continueOnError">Indicates whether subsequent actions should continue executing if an error occurs</param>
    /// <param name="args">An argument object to pass to the command action.</param>
    public CommandActionRegistry Schedule<TCommand, TArgs>(int priority, bool continueOnError, TArgs args)
        where TCommand : CommandAction<TArgs>
        where TArgs : class, new()
    {
        _invocationQueue.Enqueue(new Invocation(typeof(TCommand), args, continueOnError), priority);
        return this;
    }

    /// <summary>
    /// Attempts to schedule a command action with the specified priority and arguments.
    /// If the command is already scheduled (even with a different priority), it will not be scheduled again.
    /// </summary>
    /// <param name="priority">The priority level of the command, where lower numbers indicate higher priority.</param>
    /// <param name="continueOnError">Indicates whether the execution should continue if the command fails.</param>
    /// <param name="type">The type of the command action to schedule.</param>
    /// <param name="args">An array of arguments to pass to the command action.</param>
    /// <returns>Returns true if the command is successfully scheduled; otherwise, false if a command of the same type is already scheduled.</returns>
    public bool TrySchedule(int priority, bool continueOnError, Type type, string[] args)
    {
        if (IsCommandScheduled(type))
            return false;

        Schedule(priority, continueOnError, type, args);
        return true;
    }

    /// <summary>
    /// Attempts to schedule a command action with the specified priority and arguments.
    /// If the command is already scheduled (even with a different priority), it will not be scheduled again.
    /// </summary>
    /// <param name="priority">The priority level of the command, where lower numbers indicate higher priority.</param>
    /// <param name="continueOnError">Indicates whether the execution should continue if the command fails.</param>
    /// <returns>Returns true if the command is successfully scheduled; otherwise, false if a command of the same type is already scheduled.</returns>
    public bool TrySchedule<TCommand>(int priority, bool continueOnError)
        where TCommand : CommandAction
    {
        if (IsCommandScheduled(typeof(TCommand)))
            return false;

        Schedule<TCommand>(priority, continueOnError);
        return true;
    }

    /// <summary>
    /// Attempts to schedule a command action with the specified priority and arguments.
    /// If the command is already scheduled (even with a different priority), it will not be scheduled again.
    /// </summary>
    /// <param name="priority">The priority level of the command, where lower numbers indicate higher priority.</param>
    /// <param name="continueOnError">Indicates whether the execution should continue if the command fails.</param>
    /// <param name="args">The argument options to pass to the command action.</param>
    /// <returns>Returns true if the command is successfully scheduled; otherwise, false if a command of the same type is already scheduled.</returns>
    public bool TrySchedule<TCommand, TArgs>(int priority, bool continueOnError, TArgs args)
        where TCommand : CommandAction<TArgs>
        where TArgs : class, new()
    {
        if (IsCommandScheduled(typeof(TCommand)))
            return false;

        Schedule<TCommand, TArgs>(priority, continueOnError, args);
        return true;
    }
}
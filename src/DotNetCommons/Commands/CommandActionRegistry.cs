using System.Collections.Concurrent;
using System.Reflection;
using DotNetCommons.Sys;
using DotNetCommons.Text;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCommons.Commands;

/// <summary>
/// A registry that manages command actions and facilitates their execution. This class is designed to
/// register, resolve, and execute commands while optionally providing help functionality and other features.
/// </summary>
public class CommandActionRegistry
{
    private Action<ICommandAction, int>? _afterActionCallback;
    private Action<ICommandAction>? _beforeActionCallback;
    private readonly ConcurrentDictionary<string, Type> _commandRegistry = [];
    private readonly IServiceProvider _serviceProvider;

    public CommandActionRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public CommandActionRegistry AfterAction(Action<ICommandAction, int> action)
    {
        _afterActionCallback = action;
        return this;
    }

    public CommandActionRegistry BeforeAction(Action<ICommandAction> action)
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
        var route      = GetRoute(attr.Route, ' ');
        
        Console.WriteLine(route);
        Console.WriteLine(new string('=', route.Length));

        Console.WriteLine();
        Console.WriteLine(CommandLine.GetFormattedHelpText(optionType));

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
            x => GetRoute(x!.Route, ' '),
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
        return Execute(CommandActionOptions.Default, args);
    }

    /// <summary>
    /// Executes the registered command action with the provided array of arguments.
    /// </summary>
    /// <param name="options">How to handle resolution of commands, whether help can be automatically displayed etc.</param>
    /// <param name="args">An array of strings representing the command-line arguments passed for execution.</param>
    /// <returns>
    /// An integer value indicating the outcome of the execution. Typically, 0 indicates success, while
    /// non-zero values represent errors or specific execution conditions.
    /// </returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public int Execute(CommandActionOptions options, string[] args)
    {
        if (_commandRegistry.IsEmpty())
            throw new CommandActionResolveException("No command actions are defined.");

        var route     = args.TakeWhile(x => !x.StartsWith('/') && !x.StartsWith('-')).ToList();
        var remaining = args.Skip(route.Count).ToArray();
        var help      = route.IsEmpty() && options.HasFlag(CommandActionOptions.ShowHelpOnNoArgs);

        if (!help && options.HasFlag(CommandActionOptions.AllowHelpVerb))
        {
            if (route.Any() && route.First() == "help")
            {
                route.ExtractFirst();
                help = true;
            }

            if (route.Any() && route.Last() == "help")
            {
                route.ExtractLast();
                help = true;
            }
        }

        var commands = ResolveCommand(route);
        if (help)
        {
            if (commands.IsEmpty())
                commands = _commandRegistry.Values.ToArray();

            if (commands.IsOne())
                DisplayHelp(commands.Single());
            else
                DisplayHelp(commands);

            return 1;
        }

        if (commands.Length == 0)
            throw new CommandActionNoCommandFoundException("No such command found.");

        if (commands.Length > 1)
        {
            if (!options.HasFlag(CommandActionOptions.ShowHelpOnMultipleMatches))
                throw new CommandActionMultipleCommandsFoundException(commands);

            DisplayHelp(commands);
            return 1;
        }

        var command = commands.Single();
        return ExecuteCommand(true, command, remaining);
    }

    /// <summary>
    /// Executes the specified command action of the given type with the provided arguments.
    /// </summary>
    /// <param name="continueOnError">Whether execution should continue if the action results in an error code. If this is false,
    ///     an <see cref="CommandActionErrorResultException"/> will be thrown.</param>
    /// <param name="args">An array of strings representing the arguments passed to the command.</param>
    /// <returns>
    /// An integer representing the result of the executed command. Typically, 0 indicates successful execution,
    /// while non-zero values indicate errors or specific execution outcomes.
    /// </returns>
    public int ExecuteCommand<TCommand>(bool continueOnError, string[] args)
    {
        return ExecuteCommand(continueOnError, typeof(TCommand), args);
    }

    /// <summary>
    /// Executes a specific command action of the given type with the provided arguments.
    /// </summary>
    /// <param name="continueOnError">Whether execution should continue if the action results in an error code. If this is false,
    ///     an <see cref="CommandActionErrorResultException"/> will be thrown.</param>
    /// <param name="commandType">The type of the command action to execute. This must implement <see cref="ICommandAction"/>.</param>
    /// <param name="args">An array of strings representing the arguments passed to the command.</param>
    /// <returns>
    /// An integer representing the result of the executed command. Typically, 0 indicates success, while non-zero values indicate
    /// errors or specific execution outcomes.
    /// </returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public int ExecuteCommand(bool continueOnError, Type commandType, string[] args)
    {
        ICommandAction command;

        try
        {
            CommandLine.DisplayHelpOnEmpty = false;
            var options = CommandLine.Parse(GetActionOptionType(commandType), args);

            using var scope = _serviceProvider.CreateScope();
            command = (ICommandAction)ActivatorUtilities.CreateInstance(scope.ServiceProvider, commandType);

            var prop = commandType.GetProperty(nameof(CommandAction.Args));
            prop?.SetValue(command, options);
            prop = commandType.GetProperty(nameof(CommandAction.Registry));
            prop?.SetValue(command, this);
        }
        catch (Exception e)
        {
            throw new CommandActionException($"{commandType.Name}: {e.Message}", e);
        }

        try
        {
            _beforeActionCallback?.Invoke(command);
            var result = command.Execute();
            _afterActionCallback?.Invoke(command, result);

            if (!continueOnError && result != 0)
                throw new CommandActionErrorResultException(result);

            return result;
        }
        catch (Exception e)
        {
            _afterActionCallback?.Invoke(command, 255);
            throw new CommandActionException($"{commandType.Name}: {e.Message}", e);
        }
    }

    /// <summary>
    /// Retrieves the type of the options associated with a given command type.
    /// </summary>
    /// <param name="commandType">The <see cref="Type"/> of the command whose associated options type is to be resolved. This must inherit
    /// from a generic CommandAction&lt;&gt; type.</param>
    private static Type GetActionOptionType(Type commandType)
    {
        Type? commandActionClass = commandType;
        while (commandActionClass != null)
        {
            if (commandActionClass.IsGenericType && commandActionClass.GetGenericTypeDefinition() == typeof(CommandAction<>))
                return commandActionClass.GetGenericArguments().Single();
            
            commandActionClass = commandActionClass.BaseType;
        }
        
        throw new CommandActionResolveException($"Unable to instantiate {commandType.Name}: Generic parent CommandAction<> not found in inheritance.");
    }

    private static string GetRoute(ICollection<string> route, char delimiter = '|') => string.Join(delimiter, route
        .Where(r => r.IsSet())
        .Select(r => r.ToLower()));

    /// <summary>
    /// Registers the specified assemblies by scanning for classes that implement the ICommandAction interface
    /// and are decorated with the CommandActionAttribute. These identified command actions are added
    /// to the command registry for execution.
    /// </summary>
    /// <param name="assemblies">An array of assemblies to scan for command action classes to register.</param>
    /// <returns>
    /// The current instance of the CommandActionRegistry to allow for method chaining after registering the assemblies.
    /// </returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public CommandActionRegistry RegisterAssemblies(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var commandClasses = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsAssignableTo(typeof(ICommandAction)))
                .ToArray();

            foreach (var commandClass in commandClasses)
            {
                var attr = commandClass.GetCustomAttribute<CommandActionAttribute>();
                if (attr == null)
                    throw new CommandActionInitializationException(commandClass, "CommandAction class must have a [CommandAction] attribute.");

                var route = GetRoute(attr.Route);
                if (route.IsEmpty())
                    throw new CommandActionInitializationException(commandClass, "[CommandAction] attribute must provide a valid route.");
                
                if (!_commandRegistry.TryAdd(route, commandClass))
                    throw new CommandActionInitializationException(commandClass, "A command with the same route is already defined.");
            }
        }

        return this;
    }

    /// <summary>
    /// Registers all command actions from the calling assembly by scanning for classes
    /// that implement the ICommandAction interface and are decorated with the CommandActionAttribute.
    /// The identified command actions are automatically added to the command registry.
    /// </summary>
    /// <returns>
    /// The current instance of the CommandActionRegistry, enabling method chaining after completing
    /// the registration process for the calling assembly.
    /// </returns>
    public CommandActionRegistry RegisterThis()
    {
        return RegisterAssemblies(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Resolves and retrieves a list of command types based on the provided route sequence.
    /// </summary>
    /// <param name="route">A collection of strings representing the command route used to identify the target commands to resolve.</param>
    /// <returns>
    /// An array of <see cref="Type"/> objects representing the resolved command types that match the specified route.
    /// If no commands are found, an empty array is returned.
    /// </returns>
    private Type[] ResolveCommand(ICollection<string> route)
    {
        var search = GetRoute(route);
        return _commandRegistry
            .Where(x => x.Key.Equals(search, StringComparison.CurrentCultureIgnoreCase))
            .Select(x => x.Value)
            .ToArray();
    }
}
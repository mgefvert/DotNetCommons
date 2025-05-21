using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCommons.Sys;

public class CommandActionRegistry
{
    private readonly Dictionary<string, Type> _commandRegistry = [];
    private readonly IServiceProvider _serviceProvider;

    public CommandActionRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    private static string GetRoute(ICollection<string> route, char delimiter = '|') => string.Join(delimiter, route
        .Where(r => r.IsSet())
        .Select(r => r.ToLower()));

    public int Execute(string[] arguments)
    {
        return Execute(CommandActionOptions.Default, arguments);
    }

    public int Execute(CommandActionOptions options, string[] arguments)
    {
        if (_commandRegistry.IsEmpty())
            throw new CommandActionResolveException("No command actions are defined.");

        // TODO: Implement "help"
        var route     = arguments.TakeWhile(x => !x.StartsWith('/') && !x.StartsWith('-')).ToList();
        var remaining = arguments.Skip(route.Count).ToArray();
        var help      = false;

        if (options.HasFlag(CommandActionOptions.AllowHelp))
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
        }

        if (commands.Length == 0)
            throw new CommandActionNoCommandFoundException("No such command found.");

        if (commands.Length > 1)
        {
            if (options.HasFlag(CommandActionOptions.HandleMultipleMatches))
                DisplayHelp(commands);
            else
                throw new CommandActionMultipleCommandsFoundException(commands);
        }

        var command = commands.Single();
        return ExecuteCommand(command, remaining);
    }

    private void DisplayHelp(Type command)
    {
        var attr       = command.GetCustomAttribute<CommandActionAttribute>()!;
        var optionType = GetActionOptionType(command);
        var route      = GetRoute(attr.Route, ' ');
        
        Console.WriteLine(route);
        Console.WriteLine(new string('=', route.Length));

        Console.WriteLine(CommandLine.GetFormattedHelpText(optionType));
    }

    private void DisplayHelp(Type[] commands)
    {
        var entries = commands.Select(x => x.GetCustomAttribute<CommandActionAttribute>()).ToDictionary(
            x => GetRoute(x!.Route, ' '),
            x => x!.Description
        );
        var maxRoute = entries.Keys.Max(x => x.Length);

        foreach (var entry in entries.OrderBy(x => x.Key))
        {
            Console.WriteLine($"{entry.Key.PadRight(-maxRoute)}  {entry.Value}");
        }
    }

    private int ExecuteCommand(Type commandType, string[] args)
    {
        CommandLine.DisplayHelpOnEmpty = false;
        var options = CommandLine.Parse(GetActionOptionType(commandType), args);

        using var scope = _serviceProvider.CreateScope();
        var command = (ICommandAction)ActivatorUtilities.CreateInstance(scope.ServiceProvider, commandType);

        var argsProp = commandType.GetProperty(nameof(CommandAction.Args)) 
                       ?? throw new CommandLineException("CommandAction does not have a property named Args.");
        argsProp.SetValue(command, options);

        return command.Execute();
    }

    private static Type GetActionOptionType(Type commandType)
    {
        Type? commandActionClass = commandType;
        while (commandActionClass != null)
        {
            if (commandActionClass.IsGenericType && commandActionClass.GetGenericTypeDefinition() == typeof(CommandAction<>))
                return commandActionClass.GetGenericArguments().Single();
            
            commandActionClass = commandActionClass.BaseType;
        }
        
        throw new CommandActionResolveException($"Unable to instantiate {commandType.Name}: Generic parent CommandAction<> not found is inheritance.");
    }

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

    public CommandActionRegistry RegisterThis()
    {
        return RegisterAssemblies(Assembly.GetCallingAssembly());
    }

    public Type[] ResolveCommand(ICollection<string> route)
    {
        var search = GetRoute(route);
        return _commandRegistry
            .Where(x => x.Key.StartsWith(search))
            .Select(x => x.Value)
            .ToArray();
    }
}
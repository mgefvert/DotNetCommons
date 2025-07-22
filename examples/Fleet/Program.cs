using DotNetCommons.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Fleet;

internal class Program
{
    private static ILogger<Program> _logger = null!;
    private static ServiceProvider _serviceProvider = null!;

    private static readonly string[] Subs =
    [
        ..Enumerable.Range(1, 8).Select(i => $"ohio-{i}"),
        ..Enumerable.Range(1, 2).Select(i => $"columbia-{i}"),
        ..Enumerable.Range(1, 5).Select(i => $"los-angeles-{i}"),
    ];

    private static readonly string[] Wings =
    [
        "2nd-wing", "5th-wing", "307th-wing", "509th-wing"
    ];

    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton<UnitStates>();
        services.AddLogging(builder => builder.AddSerilog(dispose: true));

        // Build the service provider
        _serviceProvider = services.BuildServiceProvider();
        _logger          = _serviceProvider.GetRequiredService<ILogger<Program>>();
        var unitStates   = _serviceProvider.GetRequiredService<UnitStates>();
        unitStates.Load("unitStates.json", Initialize);

        try
        {
            return new CommandActionRegistry(_serviceProvider)
                .RegisterThis()
                .BeforeAction(BeforeAction)
                .AfterAction(AfterAction)
                .Execute(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        finally
        {
            unitStates.Save("unitStates.json");
        }
    }

    private static void BeforeAction(CommandAction action)
    {
        _logger.LogInformation("* Starting action {Action}", action.GetType().Name);
    }

    private static void AfterAction(CommandAction action, int result)
    {
        _logger.LogInformation("* Ending action {Action} -> {Result}", action.GetType().Name, result);
    }

    private static void Initialize(UnitStates states)
    {
        foreach (var name in Subs)
            states[name] = new UnitState(true);
        foreach (var name in Wings)
            states[name] = new UnitState(false);
    }
}
using DotNetCommons.Commands;
using Fleet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

string[] subs =
[
    ..Enumerable.Range(1, 8).Select(i => $"ohio-{i}"),
    ..Enumerable.Range(1, 2).Select(i => $"columbia-{i}"),
    ..Enumerable.Range(1, 5).Select(i => $"los-angeles-{i}"),
];

string[] wings =
[
    "2nd-wing", "5th-wing", "307th-wing", "509th-wing"
];

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var services = new ServiceCollection();
services.AddSingleton<UnitStates>();
services.AddLogging(builder => builder.AddSerilog(dispose: true));

// Build the service provider
var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var unitStates = serviceProvider.GetRequiredService<UnitStates>();
unitStates.Load("unitStates.json", Initialize);

try
{
    return new CommandActionRegistry(serviceProvider)
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

void BeforeAction(ICommandAction action) => logger.LogInformation("Starting action {Action}", action.GetType().Name);
void AfterAction(ICommandAction action, int result) => logger.LogInformation("Ending action {Action} -> {Result}", action.GetType().Name, result);

void Initialize(UnitStates states)
{
    foreach (var name in subs)
        states[name] = new UnitState(true);
    foreach (var name in wings)
        states[name] = new UnitState(false);
}

using DotNetCommons.Commands;
using Fleet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var services = new ServiceCollection();
services.AddSingleton<UnitStates>();
services.AddLogging(builder => builder.AddSerilog(dispose: true));

// Build the service provider
var serviceProvider = services.BuildServiceProvider();
var logger          = serviceProvider.GetRequiredService<ILogger<Program>>();
var unitStates      = serviceProvider.GetRequiredService<UnitStates>();
unitStates.Load("unitStates.json");

try
{
    return await new CommandActionRegistry(serviceProvider)
        .RegisterThis()
        .BeforeAction(action => logger.LogInformation("* Starting action {Action}", action.GetType().Name))
        .AfterAction((action, result) => logger.LogInformation("* Ending action {Action} -> {Result}", action.GetType().Name, result))
        .Execute(args);
}
finally
{
    unitStates.Save("unitStates.json");
}

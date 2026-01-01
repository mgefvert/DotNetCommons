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
        .BeforeAction(args => logger.LogInformation("* Starting action {Action}", args.GetType().Name))
        .AfterAction(args => logger.LogInformation("* Ending action {Action} -> {Result}", args.GetType().Name, args.ResultCode))
        .Execute(args);
}
finally
{
    unitStates.Save("unitStates.json");
}

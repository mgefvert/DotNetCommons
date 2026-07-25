using commons;
using DotNetCommons;
using DotNetCommons.Commands;
using DotNetCommons.Logging;
using DotNetCommons.Security;
using DotNetCommons.Services.Misc;
using DotNetCommons.SqlData;
using DotNetCommons.Synchronization;
using DotNetCommons.Sys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

try
{
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    var services = new ServiceCollection()
        .AddLogging(options => options
            .AddConfiguration(config.GetSection("Logging"))
            .AddCommonConsole())
        .AddSingleton<Accessor<Invocation>>()
        .AddSingleton<IConfiguration>(config)
        .AddSingleton<IpifyIntegration>()
        .AddSingleton<MySqlCnfReader>()
        .AddSingleton<ISqlDataService, SqlDataService>()
        .AddDbContext<SqlDataContext>((svc, options) => ConfigureContext(svc, options, "sqldata"))
        .BuildServiceProvider();

    return await new CommandActionRegistry(services)
        .RegisterThis()
        .BeforeInvocation(args => services.GetRequiredService<Accessor<Invocation>>().Replace(args))
        .Execute(args);
}
catch (Exception e)
{
    using (new SetConsoleColor(ConsoleColor.Red))
        Console.WriteLine($"{e.GetType().Name}: {e.Message}");
    return 1;
}

void ConfigureContext(IServiceProvider services, DbContextOptionsBuilder options, string database)
{
    var currentAction = services.GetRequiredService<Accessor<Invocation>>().GetOrThrow();
    var connection = (currentAction.Options as ConnectionArgs)?.Connection;
    if (connection.IsEmpty())
        throw new MessageException("-c | --connection <login-path> is required");

    var mysqlCnfReader = services.GetRequiredService<MySqlCnfReader>();
    var connectionString = mysqlCnfReader.RequireConnectionString(connection, database);
    options.UseMySQL(connectionString);
}

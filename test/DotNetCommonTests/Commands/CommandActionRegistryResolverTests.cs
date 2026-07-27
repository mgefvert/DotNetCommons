using DotNetCommons.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCommonTests.Commands;

// Very specific test class that ensures that commands must resolve cleanly and not short-circuit each other

[TestClass]
public class CommandActionRegistryResolverTests
{
    [CommandAction(["cmd", "one"], "Command one", [])]
    private class CommandOne : CommandAction;

    [CommandAction(["cmd", "one", "subcmd"], "Command one subcommand", [])]
    private class CommandOneSubcmd : CommandAction;

    [CommandAction(["cmd", "two"], "Command two", [])]
    private class CommandTwo : CommandAction;

    [TestMethod]
    public void SubCommands_Must_Resolve()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var registry = new CommandActionRegistry(services);

        Assert.ThrowsExactly<CommandActionResolveException>(() =>
            registry.RegisterCommand(typeof(CommandOne), typeof(CommandOneSubcmd), typeof(CommandTwo)));
    }
}

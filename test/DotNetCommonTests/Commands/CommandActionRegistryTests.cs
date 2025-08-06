using DotNetCommons.Commands;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests.Commands;

[TestClass]
public class CommandActionRegistryTests
{
    private ServiceProvider _serviceProvider = null!;
    private CommandActionRegistry _registry = null!;
    private TestReporter _testReporter = null!;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestReporter>();
        _serviceProvider = services.BuildServiceProvider();

        _registry = new CommandActionRegistry(_serviceProvider);
        _registry.RegisterThis();

        _testReporter = _serviceProvider.GetRequiredService<TestReporter>();
    }

    [TestMethod]
    public void Resolve_GlobalHelp()
    {
        var result = _registry.Resolve(["help"]);
        result.help.Should().BeTrue();
        result.commands.Length.Should().Be(3);
        result.remaining.Should().BeEmpty();
    }

    [TestMethod]
    public void Resolve_InvalidCommands()
    {
        var result = _registry.Resolve(["foobar"]);
        result.help.Should().BeFalse();
        result.commands.Should().BeEmpty();
        result.remaining.Should().BeEmpty();
    }

    [TestMethod]
    public void Resolve_WithValidCommands_ReturnsCorrectTypes()
    {
        var result = _registry.Resolve(["command", "one"]);
        result.help.Should().BeFalse();
        result.commands.Should().BeEquivalentTo([typeof(CommandOne)]);
        result.remaining.Should().BeEmpty();

        result = _registry.Resolve(["command", "two", "-v", "1"]);
        result.help.Should().BeFalse();
        result.commands.Should().BeEquivalentTo([typeof(CommandTwo)]);
        result.remaining.Should().BeEquivalentTo("-v", "1");

        result = _registry.Resolve(["command"]);
        result.help.Should().BeFalse();
        result.commands.Should().BeEquivalentTo([typeof(CommandOne), typeof(CommandTwo)]);
        result.remaining.Should().BeEmpty();

        result = _registry.Resolve(["test"]);
        result.help.Should().BeFalse();
        result.commands.Should().BeEquivalentTo([typeof(CommandTest)]);
        result.remaining.Should().BeEmpty();
    }

    [TestMethod]
    public void Resolve_WithHelp()
    {
        var result = _registry.Resolve(["help", "command", "one"]);
        result.help.Should().BeTrue();
        result.commands.Should().BeEquivalentTo([typeof(CommandOne)]);
        result.remaining.Should().BeEmpty();

        result = _registry.Resolve(["help", "command"]);
        result.help.Should().BeTrue();
        result.commands.Should().BeEquivalentTo([typeof(CommandOne), typeof(CommandTwo)]);
        result.remaining.Should().BeEmpty();

        result = _registry.Resolve(["test", "help"]);
        result.help.Should().BeTrue();
        result.commands.Should().BeEquivalentTo([typeof(CommandTest)]);
        result.remaining.Should().BeEmpty();
    }

    [TestMethod]
    public void Execute_CommandOne_Successful()
    {
        var result = _registry.Execute(["command", "one"]);
        result.Should().Be(0);
        _testReporter.Text.Should().Be("CommandOne");
    }

    [TestMethod]
    public void Execute_CommandTest_Successful()
    {
        var result = _registry.Execute(["test"]);
        result.Should().Be(0);
        _testReporter.Text.Should().Be("CommandTest;CommandOne;CommandTwo:0");
    }

    [TestMethod]
    public void Execute_CommandTest_JobAlreadyScheduled()
    {
        _registry.Schedule<CommandTest>(10, true);
        _registry.Schedule<CommandOne>(99, true);

        var result = _registry.ExecuteScheduler();
        result.Should().Be(0);
        _testReporter.Text.Should().Be("CommandTest;CommandTwo:0;CommandOne");
    }

    [TestMethod]
    public void Execute_CommandTest_BreaksOnErrorIfNoContinueSet()
    {
        _registry.Schedule<CommandTest>(10, true);
        _registry.Schedule<CommandTwo, ReturnValueArgs>(60, false, new ReturnValueArgs(1));

        var result = _registry.ExecuteScheduler();
        result.Should().Be(1);
        _testReporter.Text.Should().Be("CommandTest;CommandTwo:1");
    }

    [TestMethod]
    public void Execute_CommandTest_DoesNotBreakOnErrorIfContinue()
    {
        _registry.Schedule<CommandTest>(10, true);
        _registry.Schedule<CommandTwo, ReturnValueArgs>(60, true, new ReturnValueArgs(1));

        var result = _registry.ExecuteScheduler();
        result.Should().Be(0);
        _testReporter.Text.Should().Be("CommandTest;CommandTwo:1;CommandOne");
    }
}
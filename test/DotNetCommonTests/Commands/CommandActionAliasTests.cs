using System.Reflection;
using System.Reflection.Emit;
using DotNetCommons.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetCommonTests.Commands;

[TestClass]
[DoNotParallelize]
public class CommandActionAliasTests
{
    private ServiceProvider _services = null!;
    private CommandActionRegistry _registry = null!;
    private TestReporter _reporter = null!;
    private TextWriter _originalOutput = null!;
    private TextWriter _originalError = null!;
    private StringWriter _output = null!;
    private StringWriter _error = null!;

    [TestInitialize]
    public void Setup()
    {
        _reporter = new TestReporter();
        _services = new ServiceCollection().AddSingleton(_reporter).BuildServiceProvider();
        _registry = new CommandActionRegistry(_services);
        _registry.RegisterCommand(typeof(AliasCommand), typeof(OtherCommand));
        _originalOutput = Console.Out;
        _originalError = Console.Error;
        _output = new StringWriter();
        _error = new StringWriter();
        Console.SetOut(_output);
        Console.SetError(_error);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Console.SetOut(_originalOutput);
        Console.SetError(_originalError);
        _output.Dispose();
        _error.Dispose();
        _services.Dispose();
    }

    [TestMethod]
    [DataRow("ar", "reflect-holds")]
    [DataRow("AR", "REFLECT-HOLDS")]
    [DataRow("reflect", "legacy")]
    public void Resolve_Alias_ReturnsCanonicalCommandAndArguments(string first, string second)
    {
        var result = _registry.Resolve([first, second, "-v", "7"]);

        CollectionAssert.AreEqual(new[] { typeof(AliasCommand) }, result.commands);
        CollectionAssert.AreEqual(new[] { "-v", "7" }, result.remaining);
        Assert.IsFalse(result.help);
        Assert.IsFalse(result.routeWasEmpty);
        Assert.AreEqual("", _error.ToString());
        Assert.AreEqual("", _output.ToString());
    }

    [TestMethod]
    [DataRow("ar")]
    [DataRow("ar", "reflect")]
    [DataRow("ar", "reflect-holds", "extra")]
    public void Resolve_PartialOrExtendedAlias_DoesNotMatch(params string[] route)
    {
        Assert.IsEmpty(_registry.Resolve(route).commands);
    }

    [TestMethod]
    [DataRow("help")]
    [DataRow("reflect")]
    public void Resolve_Overview_ContainsOnlyCanonicalCommands(string route)
    {
        var result = _registry.Resolve([route]);

        CollectionAssert.AreEquivalent(new[] { typeof(AliasCommand), typeof(OtherCommand) }, result.commands);
    }

    [TestMethod]
    [DataRow("help")]
    [DataRow("reflect")]
    public async Task Execute_Overview_HidesAliases(string route)
    {
        await _registry.Execute([route]);

        var output = _output.ToString();
        StringAssert.Contains(output, "reflect gphold");
        StringAssert.Contains(output, "reflect other");
        Assert.AreEqual(2, output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.IsFalse(output.Contains("reflect-holds"));
        Assert.IsFalse(output.Contains("legacy"));
        Assert.IsFalse(output.Contains("gp" + Environment.NewLine));
        Assert.AreEqual("", _error.ToString());
    }

    [TestMethod]
    public async Task Execute_DeprecatedAlias_WarnsOnceAndPreservesArgumentsAndExitCode()
    {
        var result = await _registry.Execute(["AR", "REFLECT-HOLDS", "-v", "7"]);

        Assert.AreEqual(7, result);
        Assert.AreEqual("CommandTwo:7", _reporter.Text);
        Assert.AreEqual("Warning: Deprecated command name 'ar reflect-holds'; use 'reflect gphold'." +
                        Environment.NewLine, _error.ToString());
        Assert.AreEqual("", _output.ToString());
    }

    [TestMethod]
    public async Task Execute_DeprecatedAlias_UsesLoggerInsteadOfConsoleError()
    {
        var logger = new RecordingLogger();
        var registry = new CommandActionRegistry(_services, new DotNetCommonsCommandLineParser(), logger);
        registry.RegisterCommand(typeof(AliasCommand));

        Assert.AreEqual(0, await registry.Execute(["ar", "reflect-holds"]));

        Assert.HasCount(1, logger.Entries);
        Assert.AreEqual(LogLevel.Warning, logger.Entries[0].Level);
        Assert.AreEqual("Deprecated command name 'ar reflect-holds'; use 'reflect gphold'.",
            logger.Entries[0].Message);
        Assert.AreEqual("", _error.ToString());
    }

    [TestMethod]
    [DataRow("reflect", "gphold")]
    [DataRow("gp")]
    [DataRow("reflect", "legacy")]
    public async Task Execute_CanonicalOrNonDeprecatedAlias_DoesNotWarn(params string[] route)
    {
        Assert.AreEqual(0, await _registry.Execute(route));
        Assert.AreEqual("CommandTwo:0", _reporter.Text);
        Assert.AreEqual("", _error.ToString());
        Assert.AreEqual("", _output.ToString());
    }

    // [TestMethod]
    // [DataRow("help", "ar", "reflect-holds")]
    // [DataRow("ar", "reflect-holds", "help")]
    // public async Task Execute_AliasHelp_ShowsCanonicalDetailedHelp(params string[] route)
    // {
    //     Assert.AreEqual(1, await _registry.Execute(route));
    //
    //     StringAssert.StartsWith(_output.ToString(), "reflect gphold" + Environment.NewLine);
    //     Assert.IsFalse(_output.ToString().Contains("ar reflect-holds"));
    //     StringAssert.Contains(_error.ToString(), "use 'reflect gphold'");
    //     Assert.AreEqual("", _reporter.Text);
    // }

    [TestMethod]
    public async Task Schedule_ByType_DoesNotWarn()
    {
        _registry.Schedule<AliasCommand, ReturnValueArgs>(10, false, new ReturnValueArgs(7));

        Assert.AreEqual(7, await _registry.ExecuteScheduler());
        Assert.AreEqual("CommandTwo:7", _reporter.Text);
        Assert.AreEqual("", _error.ToString());
    }

    [TestMethod]
    public async Task Execute_DefaultCommand_DoesNotWarn()
    {
        _registry.SetDefaultCommand(typeof(AliasCommand));

        Assert.AreEqual(0, await _registry.Execute([]));
        Assert.AreEqual("CommandTwo:0", _reporter.Text);
        Assert.AreEqual("", _error.ToString());
    }

    [TestMethod]
    public async Task Execute_AfterPreprocessing_RecognizesAlias()
    {
        _registry.PreprocessArgs(args => args.InsertRange(0, ["ar", "reflect-holds"]));

        Assert.AreEqual(0, await _registry.Execute([]));
        StringAssert.Contains(_error.ToString(), "Deprecated command name");
    }

    [TestMethod]
    public void RegisterAssemblies_RegistersAliases()
    {
        // Other test fixtures deliberately contain conflicting routes, so use an isolated assembly.
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("AliasCommands"),
            AssemblyBuilderAccess.Run);
        var builder = assembly.DefineDynamicModule("Commands").DefineType("AssemblyCommand",
            TypeAttributes.Public, typeof(CommandAction));
        builder.SetCustomAttribute(new CustomAttributeBuilder(
            typeof(CommandActionAttribute).GetConstructor([typeof(string[]), typeof(string), typeof(string[])])!,
            [new[] { "canonical" }, "Assembly command", Array.Empty<string>()]));
        builder.SetCustomAttribute(new CustomAttributeBuilder(
            typeof(CommandActionAliasAttribute).GetConstructor([typeof(string[])])!, [new[] { "old" }]));
        var command = builder.CreateType()!;
        var registry = new CommandActionRegistry(_services).RegisterAssemblies(assembly);

        CollectionAssert.AreEqual(new[] { command }, registry.Resolve(["old"]).commands);
        CollectionAssert.AreEqual(new[] { command }, registry.Resolve(["help"]).commands);
    }

    [TestMethod]
    [DataRow(typeof(DuplicateCanonicalCommand))]
    [DataRow(typeof(DuplicateAliasCommand))]
    public void Register_DuplicateRoutes_RejectsBothRegistrationOrders(Type conflictingCommand)
    {
        var registry = new CommandActionRegistry(_services);
        Assert.ThrowsExactly<CommandActionInitializationException>(() =>
            registry.RegisterCommand(typeof(AliasCommand), conflictingCommand));

        registry = new CommandActionRegistry(_services);
        Assert.ThrowsExactly<CommandActionInitializationException>(() =>
            registry.RegisterCommand(conflictingCommand, typeof(AliasCommand)));
    }

    [TestMethod]
    [DataRow(typeof(ParentCanonicalCommand))]
    [DataRow(typeof(ChildCanonicalCommand))]
    [DataRow(typeof(ParentAliasCommand))]
    [DataRow(typeof(ChildAliasCommand))]
    public void Register_PrefixConflicts_RejectsBothRegistrationOrders(Type conflictingCommand)
    {
        var registry = new CommandActionRegistry(_services);
        Assert.ThrowsExactly<CommandActionResolveException>(() =>
            registry.RegisterCommand(typeof(AliasCommand), conflictingCommand));

        registry = new CommandActionRegistry(_services);
        Assert.ThrowsExactly<CommandActionResolveException>(() =>
            registry.RegisterCommand(conflictingCommand, typeof(AliasCommand)));
    }

    [TestMethod]
    [DataRow(typeof(EmptyAliasCommand))]
    [DataRow(typeof(SelfDuplicateCommand))]
    [DataRow(typeof(RepeatedAliasCommand))]
    public void Register_InvalidAlias_Throws(Type command)
    {
        Assert.ThrowsExactly<CommandActionInitializationException>(() => _registry.RegisterCommand(command));
    }

    [TestMethod]
    public void Register_AliasPrefixOfOwnCanonicalRoute_Throws()
    {
        Assert.ThrowsExactly<CommandActionResolveException>(() =>
            _registry.RegisterCommand(typeof(SelfPrefixCommand)));
    }

    [TestMethod]
    public void Register_DerivedCommand_DoesNotInheritAliases()
    {
        var registry = new CommandActionRegistry(_services);
        registry.RegisterCommand(typeof(DerivedCommand));

        Assert.IsEmpty(registry.Resolve(["ar", "reflect-holds"]).commands);
        CollectionAssert.AreEqual(new[] { typeof(DerivedCommand) }, registry.Resolve(["derived"]).commands);
    }

    [CommandAction(["reflect", "gphold"], "GP hold file", [])]
    [CommandActionAlias(["ar", "reflect-holds"], Deprecated = true)]
    [CommandActionAlias(["gp"])]
    [CommandActionAlias(["reflect", "legacy"])]
    public class AliasCommand(TestReporter reporter) : CommandTwo(reporter);

    [CommandAction(["reflect", "other"], "Other command", [])]
    private class OtherCommand : CommandAction;

    [CommandAction(["AR", "REFLECT-HOLDS"], "Duplicate", [])]
    private class DuplicateCanonicalCommand : CommandAction;

    [CommandAction(["duplicate"], "Duplicate", [])]
    [CommandActionAlias(["AR", "REFLECT-HOLDS"])]
    private class DuplicateAliasCommand : CommandAction;

    [CommandAction(["ar"], "Parent", [])]
    private class ParentCanonicalCommand : CommandAction;

    [CommandAction(["ar", "reflect-holds", "child"], "Child", [])]
    private class ChildCanonicalCommand : CommandAction;

    [CommandAction(["parent"], "Parent alias", [])]
    [CommandActionAlias(["AR"])]
    private class ParentAliasCommand : CommandAction;

    [CommandAction(["child"], "Child alias", [])]
    [CommandActionAlias(["AR", "REFLECT-HOLDS", "child"])]
    private class ChildAliasCommand : CommandAction;

    [CommandAction(["empty"], "Empty", [])]
    [CommandActionAlias([])]
    private class EmptyAliasCommand : CommandAction;

    [CommandAction(["self"], "Self", [])]
    [CommandActionAlias(["SELF"])]
    private class SelfDuplicateCommand : CommandAction;

    [CommandAction(["repeated"], "Repeated", [])]
    [CommandActionAlias(["alias"])]
    [CommandActionAlias(["ALIAS"])]
    private class RepeatedAliasCommand : CommandAction;

    [CommandAction(["self", "child"], "Self prefix", [])]
    [CommandActionAlias(["self"])]
    private class SelfPrefixCommand : CommandAction;

    [CommandAction(["derived"], "Derived", [])]
    public class DerivedCommand(TestReporter reporter) : AliasCommand(reporter);

    private class RecordingLogger : ILogger<CommandActionRegistry>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}

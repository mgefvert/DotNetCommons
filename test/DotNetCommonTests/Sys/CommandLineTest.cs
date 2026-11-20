using DotNetCommons.Sys;

using DotNetCommons.Commands;

namespace DotNetCommonTests.Sys;

[TestClass]
public class CommandLineTest
{
    [TestMethod]
    [DataRow(new[] { "-u", "test", "-p", "password", "-P", "80", "-z" })]
    [DataRow(new[] { "-u=test", "-p=password", "-P=80", "-z" })]
    [DataRow(new[] { "/u", "test", "/p", "password", "/P", "80", "/z" })]
    [DataRow(new[] { "/u=test", "/p=password", "-P=80", "/z" })]
    [DataRow(new[] { "--user", "test", "--password", "password", "--port", "80", "--zip" })]
    [DataRow(new[] { "--user=test", "--password=password", "--port", "80", "--zip" })]
    public void TestOptionSyntax(string[] args)
    {
        var options = CommandLine.Parse<Options>(args);
        Assert.AreEqual("test", options.User);
        Assert.AreEqual("password", options.Password);
        Assert.AreEqual(80, options.Port);
        Assert.IsTrue(options.Zip);
        Assert.IsFalse(options.Encrypt);
    }

    [TestMethod]
    public void TestMultipleShortOptions()
    {
        var settings = new CommandLineSettings { MultipleShortOptions = false };
        var options = CommandLine.Parse<Options>(settings, "-uroot");
        Assert.AreEqual("root", options.User);

        options = CommandLine.Parse<Options>("-ze");
        Assert.IsTrue(options.Zip);
        Assert.IsTrue(options.Encrypt);
    }

    [TestMethod]
    public void TestCommandActionParserDoesNotChangeDefaultHelpBehavior()
    {
        var parser = new DotNetCommonsCommandLineParser();
        var options = (Options)parser.Parse(typeof(Options), []);
        Assert.IsNull(options.User);

        Assert.ThrowsExactly<CommandLineDisplayHelpException>(() => CommandLine.Parse<Options>([]));
    }

    [TestMethod]
    public void TestEndOfOptions()
    {
        var options = CommandLine.Parse<Options>("--zip", "--", "-draft.txt", "--encrypt", "/tmp/input", "--");
        Assert.IsTrue(options.Zip);
        Assert.IsFalse(options.Encrypt);
        Assert.AreEqual("-draft.txt", options.Param1);
        Assert.AreEqual("--encrypt", options.Param2);
        CollectionAssert.AreEqual(new[] { "/tmp/input", "--" }, options.Params);
    }

    [TestMethod]
    [DataRow("/tmp/input")]
    [DataRow("--encrypt")]
    [DataRow("--")]
    public void TestRequiredValuesThatLookLikeOptions(string value)
    {
        var options = CommandLine.Parse<Options>("--user", value, "-P", "-5", "--zip");
        Assert.AreEqual(value, options.User);
        Assert.AreEqual(-5, options.Port);
        Assert.IsTrue(options.Zip);
        Assert.IsFalse(options.Encrypt);
    }

    [TestMethod]
    [DataRow("--missing", "missing", CommandLineParameterError.UndefinedParameter)]
    [DataRow("--port", "port", CommandLineParameterError.ValueRequired)]
    [DataRow("--zip=true", "zip", CommandLineParameterError.BooleanParameterDoesNotTakeValue)]
    public void TestOptionErrors(string arg, string parameter, CommandLineParameterError error)
    {
        var exception = Assert.ThrowsExactly<CommandLineParameterException>(() => CommandLine.Parse<Options>(arg));
        Assert.AreEqual(parameter, exception.Parameter);
        Assert.AreEqual(error, exception.Error);
    }

    [TestMethod]
    public void TestPositions()
    {
        var options = CommandLine.Parse<Options>("-z", "p1", "p2", "p3", "-e", "p4");
        Assert.IsTrue(options.Zip);
        Assert.IsTrue(options.Encrypt);
        Assert.AreEqual("p1", options.Param1);
        Assert.AreEqual("p2", options.Param2);
        Assert.AreEqual("p3,p4", string.Join(",", options.Params));
    }

    [TestMethod]
    public void TestValidation()
    {
        var options = CommandLine.Parse<OptionsRequired>("-u", "test", "-p", "password");
        Assert.AreEqual("test", options.User);
        Assert.AreEqual("password", options.Password);

        options = CommandLine.Parse<OptionsRequired>("-u", "test");
        Assert.AreEqual("test", options.User);
        Assert.IsNull(options.Password);

        Assert.ThrowsExactly<CommandLineParameterException>(() => CommandLine.Parse<OptionsRequired>("-p", "password"));
    }

    [TestMethod]
    public void TestPositionalOnlyHelp()
    {
        var help = CommandLine.GetFormattedHelpText(typeof(PositionalOptions));

        CollectionAssert.AreEqual(new[] { "Positional arguments:", "<Source>", "[Destination]", "[Extras...]" },
            help.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    [TestMethod]
    public void TestRequiredValueOptions()
    {
        var missingMode = Assert.ThrowsExactly<CommandLineParameterException>(
            () => CommandLine.Parse<RequiredValueOptions>("--count", "0"));
        Assert.AreEqual("--mode", missingMode.Parameter);
        Assert.AreEqual(CommandLineParameterError.ValueRequired, missingMode.Error);

        var missingCount = Assert.ThrowsExactly<CommandLineParameterException>(
            () => CommandLine.Parse<RequiredValueOptions>("--mode", "0"));
        Assert.AreEqual("--count", missingCount.Parameter);
        Assert.AreEqual(CommandLineParameterError.ValueRequired, missingCount.Error);

        var options = CommandLine.Parse<RequiredValueOptions>("--mode", "0", "--count", "0");
        Assert.AreEqual((OptionMode)0, options.Mode);
        Assert.AreEqual(0, options.Count);
    }

    [TestMethod]
    public void TestEnumValues()
    {
        var options = CommandLine.Parse<EnumOptions>("--mode", "XYZ", "--optional-mode=read-only",
            "--modes", "abc", "--modes=5", "--modes", "-2");

        Assert.AreEqual(OptionMode.Xyz, options.Mode);
        Assert.AreEqual(OptionMode.ReadOnly, options.OptionalMode);
        CollectionAssert.AreEqual(new[] { OptionMode.Abc, (OptionMode)5, (OptionMode)(-2) }, options.Modes);
    }

    [TestMethod]
    [DataRow("--mode", "oops")]
    [DataRow("--optional-mode", "")]
    [DataRow("--modes", "oops")]
    public void TestInvalidEnumValues(string option, string value)
    {
        var exception = Assert.ThrowsExactly<CommandLineParameterException>(
            () => CommandLine.Parse<EnumOptions>(option, value));

        Assert.AreEqual(option, exception.Parameter);
        Assert.AreEqual(CommandLineParameterError.InvalidValue, exception.Error);
        StringAssert.Contains(exception.Message, $"'{value}'");
        StringAssert.Contains(exception.Message, "Abc, Xyz, ReadOnly");
    }
}

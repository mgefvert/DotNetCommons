using DotNetCommons.Sys;

namespace DotNetCommonTests.Sys;

[TestClass]
public class CommandLineTest
{
    [TestMethod]
    public void TestShortOptions()
    {
        var options = CommandLine.Parse<Options>("-u", "test", "-p", "password", "-P", "80", "-z");
        Assert.AreEqual("test", options.User);
        Assert.AreEqual("password", options.Password);
        Assert.AreEqual(80, options.Port);
        Assert.IsTrue(options.Zip);
        Assert.IsFalse(options.Encrypt);

        options = CommandLine.Parse<Options>("-u=test", "-p=password", "-P=80", "-z");
        Assert.AreEqual("test", options.User);
        Assert.AreEqual("password", options.Password);
        Assert.AreEqual(80, options.Port);
        Assert.IsTrue(options.Zip);
        Assert.IsFalse(options.Encrypt);
    }

    [TestMethod]
    public void TestShortOptionsAlternate()
    {
        var options = CommandLine.Parse<Options>("/u", "test", "/p", "password", "/P", "80", "/z");
        Assert.AreEqual("test", options.User);
        Assert.AreEqual("password", options.Password);
        Assert.AreEqual(80, options.Port);
        Assert.IsTrue(options.Zip);
        Assert.IsFalse(options.Encrypt);

        options = CommandLine.Parse<Options>("/u=test", "/p=password", "-P=80", "/z");
        Assert.AreEqual("test", options.User);
        Assert.AreEqual("password", options.Password);
        Assert.AreEqual(80, options.Port);
        Assert.IsTrue(options.Zip);
        Assert.IsFalse(options.Encrypt);
    }

    [TestMethod]
    public void TestLongOptions()
    {
        var options = CommandLine.Parse<Options>("--user", "test", "--password", "password", "--port", "80", "--zip");
        Assert.AreEqual("test", options.User);
        Assert.AreEqual("password", options.Password);
        Assert.AreEqual(80, options.Port);
        Assert.IsTrue(options.Zip);
        Assert.IsFalse(options.Encrypt);

        options = CommandLine.Parse<Options>("--user=test", "--password=password", "--port", "80", "--zip");
        Assert.AreEqual("test", options.User);
        Assert.AreEqual("password", options.Password);
        Assert.AreEqual(80, options.Port);
        Assert.IsTrue(options.Zip);
        Assert.IsFalse(options.Encrypt);
    }

    [TestMethod]
    public void TestMultipleShortOptions()
    {
        CommandLine.MultipleShortOptions = true;

        var options = CommandLine.Parse<Options>("-ze");
        Assert.IsTrue(options.Zip);
        Assert.IsTrue(options.Encrypt);

        CommandLine.MultipleShortOptions = false;

        options = CommandLine.Parse<Options>("-uroot");
        Assert.AreEqual("root", options.User);
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
}
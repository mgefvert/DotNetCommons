using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCommons.Sys;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Test.Sys;

[TestClass]
public class SpawnTest
{
    [TestMethod]
    public void Run_Works()
    {
        var result = new Spawn("cmd /c echo Hello World!").Run();

        result.Command.Should().Be("cmd");
        result.Parameters.Should().Be("/c echo Hello World!");
        result.ExitCode.Should().Be(0);
        result.IsFinished.Should().BeTrue();
        result.IsRunning.Should().BeFalse();
        result.Output.Count.Should().Be(1);
        result.Output[0].Should().Be("Hello World!");
        result.Text.Should().Be("Hello World!");
    }

    [TestMethod]
    public void Run_WithEcho_Works()
    {
        Console.WriteLine("There should be 'dir' output below:");
        new Spawn("cmd /c dir").WithEcho().Run();
    }

    [TestMethod]
    public void Run_WithRedirectInput_Works()
    {
        var result = new Spawn("cmd /c pause").WithRedirectInput().Start();

        result.IsRunning.Should().BeTrue();
        result.IsFinished.Should().BeFalse();

        result.InputStream!.Write('X');
        Thread.Sleep(100);
        result.IsRunning.Should().BeFalse();
        result.IsFinished.Should().BeTrue();
    }

    [TestMethod]
    public void Run_WithStartDirectory_Works()
    {
        var result = new Spawn("cmd /c cd").WithStartDirectory("c:\\windows").Run();
        result.Text.Should().Be("c:\\windows");
    }

    [TestMethod]
    public async Task RunAsync_Works()
    {
        var result = await new Spawn("cmd /c echo Hello World!").RunAsync();

        result.Command.Should().Be("cmd");
        result.Parameters.Should().Be("/c echo Hello World!");
        result.ExitCode.Should().Be(0);
        result.IsFinished.Should().BeTrue();
        result.IsRunning.Should().BeFalse();
        result.Output.Count.Should().Be(1);
        result.Output[0].Should().Be("Hello World!");
        result.Text.Should().Be("Hello World!");
    }

    [TestMethod]
    public void Wait_TimeoutAndKillWorks()
    {
        var result = new Spawn("cmd /c pause").Start();

        for (var i = 0; i < 10; i++)
        {
            if (result.IsRunning)
                break;

            Thread.Sleep(1000);
        }

        result.IsRunning.Should().BeTrue();
        result.ExitCode.Should().BeNull();
        result.Kill();

        result.Wait(TimeSpan.FromSeconds(10));

        result.IsRunning.Should().BeFalse();
        result.ExitCode.Should().NotBeNull();
    }
}
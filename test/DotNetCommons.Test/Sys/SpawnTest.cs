using DotNetCommons.Sys;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Test.Sys;

[TestClass]
public class SpawnTest
{
    private bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    [TestMethod]
    public void Run_Works()
    {
        var cmd = IsLinux ? "echo Hello World!" : "cmd /c echo Hello World!";
        var result = new Spawn(cmd).Run();

        if (IsLinux)
        {
            result.Command.Should().Be("echo");
            result.Parameters.Should().Be("Hello World!");
        }
        else
        {
            result.Command.Should().Be("cmd");
            result.Parameters.Should().Be("/c echo Hello World!");
        }

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
        var result = new Spawn(IsLinux ? "ls -a" : "cmd /c dir").WithEcho().Run();
        result.Output.Should().NotBeEmpty();
    }

    [TestMethod]
    public void Run_WithRedirectInput_Works()
    {
        var result = new Spawn(IsLinux ? "bash -c \"read -p 'Press any key to continue...' -n 1 -s\"" : "cmd /c pause").WithRedirectInput().Start();

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
        var result = IsLinux
            ? new Spawn("pwd").WithStartDirectory("/tmp").Run()
            : new Spawn("cmd /c cd").WithStartDirectory("c:\\windows").Run();

        result.Text.ToLower().Should().Be(IsLinux ? "/tmp" : "c:\\windows");
    }

    [TestMethod]
    public async Task RunAsync_Works()
    {
        var result = await new Spawn(IsLinux ? "echo Hello World!" : "cmd /c echo Hello World!").RunAsync();

        if (IsLinux)
        {
            result.Command.Should().Be("echo");
            result.Parameters.Should().Be("Hello World!");
        }
        else
        {
            result.Command.Should().Be("cmd");
            result.Parameters.Should().Be("/c echo Hello World!");
        }

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
        Console.WriteLine($"{nameof(Wait_TimeoutAndKillWorks)}: Starting");
        var spawn = new Spawn(IsLinux ? "bash -c read" : "cmd /c pause");
        Console.WriteLine($"{nameof(Wait_TimeoutAndKillWorks)}: {spawn.Command} {spawn.Parameters}");
        var result = spawn.Start();

        for (var i = 0; i < 10; i++)
        {
            Console.WriteLine($"{nameof(Wait_TimeoutAndKillWorks)}: IsRunning = {result.IsRunning}, ExitCode = {result.ExitCode}");
            if (result.IsRunning)
                break;

            Thread.Sleep(1000);
        }

        result.IsRunning.Should().BeTrue();
        result.ExitCode.Should().BeNull();

        Console.WriteLine($"{nameof(Wait_TimeoutAndKillWorks)}: Attempting kill");
        result.Kill();

        for (var i = 0; i < 10; i++)
        {
            Console.WriteLine($"{nameof(Wait_TimeoutAndKillWorks)}: IsRunning = {result.IsRunning}, ExitCode = {result.ExitCode}");
            if (!result.IsRunning)
                break;

            Thread.Sleep(1000);
        }

        result.IsRunning.Should().BeFalse();
        result.ExitCode.Should().NotBeNull();
    }
}
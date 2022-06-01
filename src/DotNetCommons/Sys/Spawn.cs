using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Sys;

/// <summary>
/// Start a new process. Wraps the Process class in easy to use methods.
/// </summary>
public class Spawn : IDisposable
{
    /// <summary>
    /// Process to execute.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Echo process output to the console.
    /// </summary>
    public bool Echo { get; set; }

    /// <summary>
    /// Command-line parameters for the process.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Whether to redirect input to the process.
    /// </summary>
    public bool RedirectInput { get; set; }

    /// <summary>
    /// Optional start directory for the process.
    /// </summary>
    public string? StartDirectory { get; set; }

    /// <summary>
    /// Process object wrapping the new program.
    /// </summary>
    public Process? Process { get; private set; }

    /// <summary>
    /// Exit code from the process, or null if the process hasn't exited yet.
    /// </summary>
    public int? ExitCode => IsFinished ? Process!.ExitCode : null;

    /// <summary>
    /// Input stream available for writing to, if we have redirected the standard input.
    /// </summary>
    public StreamWriter? InputStream => Process?.StandardInput;

    /// <summary>
    /// True if the process is currently running.
    /// </summary>
    public bool IsRunning => Process is { HasExited: false };

    /// <summary>
    /// True if the process has finished.
    /// </summary>
    public bool IsFinished => Process is { HasExited: true };

    /// <summary>
    /// Output from the process as a list of strings.
    /// </summary>
    public List<string> Output { get; } = new();

    /// <summary>
    /// Output from the process as a single string.
    /// </summary>
    public string Text => string.Join(Environment.NewLine, Output);

    /// <summary>
    /// Create a Spawn object that encapsulates a given command and arguments.
    /// </summary>
    public Spawn(string command, params string[] arguments)
    {
        Command = command.Chomp(out var remains) ?? throw new ArgumentNullException(nameof(command));

        if (arguments.Any())
            remains = string.Join(" ", new[] { remains }.Concat(arguments));

        Parameters = remains;
    }

    private void AssertProcessStarted()
    {
        if (Process == null)
            throw new InvalidOperationException("Process has not been started");
    }

    /// <summary>
    /// Expands environment variables and, if unqualified, locates the exe in the working directory
    /// or the environment's path.
    /// </summary>
    /// <param name="exe">The name of the executable file</param>
    public static string? FindExePath(string exe)
    {
        exe = Environment.ExpandEnvironmentVariables(exe);

        if (File.Exists(exe))
            return Path.GetFullPath(exe);

        if (!string.IsNullOrEmpty(Path.GetDirectoryName(exe)))
            return null;

        foreach (var path in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';').TrimAndFilter())
        {
            var exePath = Path.Combine(path, exe);
            if (File.Exists(exePath))
                return Path.GetFullPath(exePath);
        }

        return null;
    }

    private void OnReceivedEventHandler(object sender, DataReceivedEventArgs args)
    {
        if (args.Data == null)
            return;

        Output.Add(args.Data);
        if (Echo)
            Console.WriteLine(args.Data);
    }

    /// <summary>
    /// Finalize process handles.
    /// </summary>
    public void Dispose()
    {
        Process?.Dispose();
        Process = null;
    }

    /// <summary>
    /// Kill the process immediately.
    /// </summary>
    public void Kill()
    {
        Process?.Kill();
    }

    /// <summary>
    /// Run the process and wait for exit.
    /// </summary>
    public Spawn Run(TimeSpan? timeout = null)
    {
        Start();
        Wait(timeout);

        return this;
    }

    /// <summary>
    /// Run the process asynchronously and wait for exit.
    /// </summary>
    public async Task<Spawn> RunAsync(TimeSpan? timeout = null)
    {
        Start();
        await WaitAsync(timeout);

        return this;
    }

    /// <summary>
    /// Start the process.
    /// </summary>
    public Spawn Start()
    {
        Process = new Process
        {
            StartInfo = new ProcessStartInfo(Command, Parameters ?? "")
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = RedirectInput,
                UseShellExecute = false,
                WorkingDirectory = StartDirectory ?? Directory.GetCurrentDirectory()
            }
        };

        Process.ErrorDataReceived += OnReceivedEventHandler;
        Process.OutputDataReceived += OnReceivedEventHandler;

        if (!Process.Start())
            throw new Exception($"No process started: {Command} {Parameters}");

        Process.BeginErrorReadLine();
        Process.BeginOutputReadLine();

        return this;
    }

    /// <summary>
    /// Wait synchronously for the process to exit.
    /// </summary>
    /// <returns>The exit code.</returns>
    public int? Wait(TimeSpan? timeout = null)
    {
        AssertProcessStarted();

        Process!.WaitForExit((int?)timeout?.TotalMilliseconds ?? -1);

        // Give the process a chance to finish up
        Thread.Sleep(10);

        Process.CancelErrorRead();
        Process.CancelOutputRead();

        return ExitCode;
    }

    /// <summary>
    /// Wait asynchronously for the process to exit.
    /// </summary>
    /// <returns>The exit code.</returns>
    public async Task<int?> WaitAsync(TimeSpan? timeout = null)
    {
        AssertProcessStarted();

        var cancel = timeout != null ? new CancellationTokenSource(timeout.Value).Token : CancellationToken.None;
        await Process!.WaitForExitAsync(cancel);

        // Give the process a chance to finish up
        // ReSharper disable once MethodSupportsCancellation
        await Task.Delay(10);

        Process.CancelErrorRead();
        Process.CancelOutputRead();

        return ExitCode;
    }

    /// <summary>
    /// Echo the output to the console.
    /// </summary>
    public Spawn WithEcho(bool echo = true)
    {
        Echo = echo;
        return this;
    }

    /// <summary>
    /// Redirect the standard input and make the InputStream property available to the caller.
    /// </summary>
    public Spawn WithRedirectInput(bool redirect = true)
    {
        RedirectInput = redirect;
        return this;
    }

    /// <summary>
    /// Start the program in a given directory.
    /// </summary>
    public Spawn WithStartDirectory(string startDirectory)
    {
        StartDirectory = startDirectory;
        return this;
    }

    /// <summary>
    /// Execute an action if the result from a child process is non-zero. Throws an exception
    /// if the process has not completed yet.
    /// </summary>
    public void IfResultNonZero(Action<Spawn> action)
    {
        if (!IsFinished)
            throw new InvalidOperationException("Process has not finished");

        if (ExitCode == 0)
            return;

        action(this);
    }
}
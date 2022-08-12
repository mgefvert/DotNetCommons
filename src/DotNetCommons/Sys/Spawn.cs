using DotNetCommons.Text;
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
    /// Command or executable to start.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Command line parameters.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Echo output to console if RedirectOutput is true. If RedirectOutput is false, this property
    /// is not used - output is always to the console.
    /// </summary>
    public bool Echo { get; set; }

    /// <summary>
    /// Redirect the standard input handle and make <seealso cref="InputStream"/> available to write to.
    /// </summary>
    public bool RedirectInput { get; set; }

    /// <summary>
    /// Redirect the standard output and error handles. Enabled by default; set to false to disable and
    /// write straight through to the console.
    /// </summary>
    public bool RedirectOutput { get; set; } = true;

    /// <summary>
    /// Start directory to use.
    /// </summary>
    public string? StartDirectory { get; set; }

    /// <summary>
    /// Process object for detailed control of the running program.
    /// </summary>
    public Process? Process { get; private set; }

    /// <summary>
    /// Optional preprocessor for the output; can be used to alter or capture the output stream before
    /// the regular processing takes over. Return null from the preprocessor to completely disable
    /// handling of that line.
    /// </summary>
    public Func<string, TextWriter, string?>? OutputPreprocessor;

    /// <summary>
    /// Exit code, if the process has finished, or null if it's still running.
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

    private void OnReceivedErrorHandler(object sender, DataReceivedEventArgs args) =>
        OnReceivedEventHandler(Console.Error, args);

    private void OnReceivedOutputHandler(object sender, DataReceivedEventArgs args) =>
        OnReceivedEventHandler(Console.Out, args);

    private void OnReceivedEventHandler(TextWriter writer, DataReceivedEventArgs args)
    {
        var data = args.Data;
        if (data == null)
            return;

        if (OutputPreprocessor != null)
            data = OutputPreprocessor(data, writer);

        if (data == null)
            return;

        Output.Add(data);
        if (Echo)
            Console.WriteLine(data);
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
                RedirectStandardError = RedirectOutput,
                RedirectStandardOutput = RedirectOutput,
                RedirectStandardInput = RedirectInput,
                UseShellExecute = false,
                WorkingDirectory = StartDirectory ?? Directory.GetCurrentDirectory()
            }
        };

        if (RedirectOutput)
        {
            Process.ErrorDataReceived += OnReceivedErrorHandler;
            Process.OutputDataReceived += OnReceivedOutputHandler;
        }

        if (!Process.Start())
            throw new Exception($"No process started: {Command} {Parameters}");

        if (RedirectOutput)
        {
            Process.BeginErrorReadLine();
            Process.BeginOutputReadLine();
        }

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

        if (RedirectOutput)
        {
            // Give the process a chance to finish up
            Thread.Sleep(10);

            Process.CancelErrorRead();
            Process.CancelOutputRead();
        }

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

        if (RedirectOutput)
        {
            // Give the process a chance to finish up
            // ReSharper disable once MethodSupportsCancellation
            await Task.Delay(10);

            Process.CancelErrorRead();
            Process.CancelOutputRead();
        }

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
    /// Redirect the standard output and error streams. Enabled by default, call with false to disable and provide
    /// a straight output-to-console output.
    /// </summary>
    public Spawn WithRedirectOutput(bool redirect = true)
    {
        RedirectOutput = redirect;
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
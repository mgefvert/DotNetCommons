using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using DotNetCommons.Sys;
using DotNetCommons.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Test.Sys
{
    [TestClass]
    public class SpawnTest
    {
        [TestMethod]
        public void TestExtractCommand()
        {
            Spawn.ExtractCommand("cmd", out var exe, out var p);
            Assert.AreEqual("cmd", exe);
            Assert.AreEqual("", p);

            Spawn.ExtractCommand("cmd hello, world", out exe, out p);
            Assert.AreEqual("cmd", exe);
            Assert.AreEqual("hello, world", p);

            Spawn.ExtractCommand("cmd \"hello, world\"", out exe, out p);
            Assert.AreEqual("cmd", exe);
            Assert.AreEqual("\"hello, world\"", p);

            Spawn.ExtractCommand("\"c:\\program files\\test.exe\"", out exe, out p);
            Assert.AreEqual("c:\\program files\\test.exe", exe);
            Assert.AreEqual("", p);

            Spawn.ExtractCommand("\"c:\\program files\\test.exe\" hello, world", out exe, out p);
            Assert.AreEqual("c:\\program files\\test.exe", exe);
            Assert.AreEqual("hello, world", p);
        }

        public static string Run(string cmd, string parameters = null, string startDirectory = null)
        {
            var startInfo = new ProcessStartInfo(cmd, parameters)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = startDirectory ?? Directory.GetCurrentDirectory()
            };

            var process = new Process { StartInfo = startInfo };

            try
            {
                var result = new StringBuilder();
                process.ErrorDataReceived += (sender, args) => result.AppendLine(args.Data);
                process.OutputDataReceived += (sender, args) => result.AppendLine(args.Data);

                if (!process.Start())
                    throw new Exception($"No process started: {cmd} {parameters}");

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit();
                Thread.Sleep(10);

                process.CancelErrorRead();
                process.CancelOutputRead();

                return result.ToString();
            }
            finally
            {
                process.Dispose();
            }
        }

        /// <summary>
        /// Expands environment variables and, if unqualified, locates the exe in the working directory
        /// or the evironment's path.
        /// </summary>
        /// <param name="exe">The name of the executable file</param>
        public static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);

            if (File.Exists(exe))
                return Path.GetFullPath(exe);

            if (!string.IsNullOrEmpty(Path.GetDirectoryName(exe)))
                return null;

            foreach (var path in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';').TrimAndFilter())
            {
                var exepath = Path.Combine(path, exe);
                if (File.Exists(exepath))
                    return Path.GetFullPath(exepath);
            }

            return null;
        }
    }
}

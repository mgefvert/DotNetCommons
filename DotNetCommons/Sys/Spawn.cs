using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using DotNetCommons.Text;

namespace DotNetCommons.Sys
{
    public class Spawn
    {
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

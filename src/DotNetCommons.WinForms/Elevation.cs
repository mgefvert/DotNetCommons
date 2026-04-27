using System.Diagnostics;
using System.Security.Principal;

namespace DotNetCommons.WinForms;

/// <summary>
/// Provides utility methods to check and handle process elevation (administrative privileges).
/// </summary>
public static class Elevation
{
    /// <summary>
    /// Attempts to restart the current process with administrative privileges.
    /// If the current process is already elevated, the method returns without taking any action.
    /// Otherwise, it attempts to elevate and relaunch the application.
    /// If elevation fails, an exception is thrown, and the application is terminated.
    /// </summary>
    /// <returns>If this method returns, it has administrative access rights. Otherwise this method will request
    /// elevated rights and terminate the current process; it will not return unless an exception is thrown.</returns>
    public static void ForkAsAdministrator()
    {
        if (IsAdministrator())
            return;

        if (!RequestElevation())
            throw new Exception("Unable to elevate this process to administrative rights.");

        Environment.Exit(0);
    }

    /// <summary>
    /// Determines whether the current process is running with administrative privileges.
    /// </summary>
    /// <returns>
    /// True if the current process is running as an administrator; otherwise, false.
    /// </returns>
    public static bool IsAdministrator()
    {
        using var identity  = WindowsIdentity.GetCurrent();
        var       principal = new WindowsPrincipal(identity);

        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// Attempts to restart the current process with elevated privileges; it forwards the same command-line arguments to the new
    /// process as this process was started with. If successful, the method will return true and the process should exit at that point.
    /// </summary>
    /// <returns>
    /// True if the elevation request was initiated successfully; false if the user declined the elevation prompt.
    /// </returns>
    public static bool RequestElevation()
    {
        var currentFileName = Process.GetCurrentProcess().MainModule?.FileName
                              ?? throw new InvalidOperationException("Unable to determine current executable file name.");

        var psi = new ProcessStartInfo
        {
            FileName        = currentFileName,
            UseShellExecute = true,
            Verb            = "runas",
            Arguments       = string.Join(" ", Environment.GetCommandLineArgs())
        };

        try
        {
            Process.Start(psi);
            return true; // exit current (non-elevated) process
        }
        catch
        {
            // User declined UAC
            return false;
        }
    }
}
using System;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

// Taken from http://stackoverflow.com/a/1172111

namespace DotNetCommons.WinForms;

public static class ServiceHelper
{
    public static WinApi.ServiceState GetServiceStatus(string serviceName)
    {
        var scm = OpenSCManager(WinApi.ScmAccessRights.Connect);

        try
        {
            var service = WinApi.OpenService(scm, serviceName, WinApi.ServiceAccessRights.QueryStatus);
            if (service == IntPtr.Zero)
                return WinApi.ServiceState.NotFound;

            try
            {
                return GetServiceStatus(service);
            }
            finally
            {
                WinApi.CloseServiceHandle(service);
            }
        }
        finally
        {
            WinApi.CloseServiceHandle(scm);
        }
    }

    private static WinApi.ServiceState GetServiceStatus(IntPtr service)
    {
        var status = new WinApi.SERVICE_STATUS();

        if (WinApi.QueryServiceStatus(service, status) == 0)
            throw new ApplicationException("Failed to query service status.");

        return status.dwCurrentState;
    }

    public static void Install(string serviceName, string displayName, string fileName, WinApi.ServiceBootFlag startFlag = WinApi.ServiceBootFlag.AutoStart)
    {
        var scm = OpenSCManager(WinApi.ScmAccessRights.AllAccess);

        try
        {
            var service = WinApi.OpenService(scm, serviceName, WinApi.ServiceAccessRights.AllAccess);

            if (service == IntPtr.Zero)
                service = WinApi.CreateService(scm, serviceName, displayName, WinApi.ServiceAccessRights.AllAccess, WinApi.SERVICE_WIN32_OWN_PROCESS,
                    startFlag, WinApi.ServiceError.Normal, fileName, null, IntPtr.Zero, null, null, null);

            if (service == IntPtr.Zero)
                throw new ApplicationException("Failed to install service.");

            WinApi.CloseServiceHandle(service);
        }
        finally
        {
            WinApi.CloseServiceHandle(scm);
        }
    }

    private static IntPtr OpenSCManager(WinApi.ScmAccessRights rights)
    {
        var scm = WinApi.OpenSCManager(null, null, rights);
        if (scm == IntPtr.Zero)
            throw new ApplicationException("Could not connect to service control manager.");

        return scm;
    }

    public static bool ServiceIsInstalled(string serviceName)
    {
        var scm = OpenSCManager(WinApi.ScmAccessRights.Connect);

        try
        {
            var service = WinApi.OpenService(scm, serviceName, WinApi.ServiceAccessRights.QueryStatus);

            if (service == IntPtr.Zero)
                return false;

            WinApi.CloseServiceHandle(service);
            return true;
        }
        finally
        {
            WinApi.CloseServiceHandle(scm);
        }
    }

    public static void StartService(string serviceName)
    {
        var scm = OpenSCManager(WinApi.ScmAccessRights.Connect);

        try
        {
            var service = WinApi.OpenService(scm, serviceName, WinApi.ServiceAccessRights.QueryStatus | WinApi.ServiceAccessRights.Start);
            if (service == IntPtr.Zero)
                throw new ApplicationException("Could not open service.");

            try
            {
                StartService(service);
            }
            finally
            {
                WinApi.CloseServiceHandle(service);
            }
        }
        finally
        {
            WinApi.CloseServiceHandle(scm);
        }
    }

    private static void StartService(IntPtr service)
    {
        WinApi.StartService(service, 0, 0);
        var changedStatus = WaitForServiceStatus(service, WinApi.ServiceState.StartPending, WinApi.ServiceState.Running);
        if (!changedStatus)
            throw new ApplicationException("Unable to start service");
    }

    public static void StopService(string serviceName)
    {
        var scm = OpenSCManager(WinApi.ScmAccessRights.Connect);

        try
        {
            var service = WinApi.OpenService(scm, serviceName, WinApi.ServiceAccessRights.QueryStatus | WinApi.ServiceAccessRights.Stop);
            if (service == IntPtr.Zero)
                throw new ApplicationException("Could not open service.");

            try
            {
                StopService(service);
            }
            finally
            {
                WinApi.CloseServiceHandle(service);
            }
        }
        finally
        {
            WinApi.CloseServiceHandle(scm);
        }
    }

    private static void StopService(IntPtr service)
    {
        var status = new WinApi.SERVICE_STATUS();
        WinApi.ControlService(service, WinApi.ServiceControl.Stop, status);
        var changedStatus = WaitForServiceStatus(service, WinApi.ServiceState.StopPending, WinApi.ServiceState.Stopped);
        if (!changedStatus)
            throw new ApplicationException("Unable to stop service");
    }

    private static bool WaitForServiceStatus(IntPtr service, WinApi.ServiceState waitStatus, WinApi.ServiceState desiredStatus)
    {
        var status = new WinApi.SERVICE_STATUS();

        WinApi.QueryServiceStatus(service, status);
        if (status.dwCurrentState == desiredStatus)
            return true;

        var dwStartTickCount = Environment.TickCount;
        var dwOldCheckPoint = status.dwCheckPoint;

        while (status.dwCurrentState == waitStatus)
        {
            // Do not wait longer than the wait hint. A good interval is
            // one tenth the wait hint, but no less than 1 second and no
            // more than 10 seconds.

            var dwWaitTime = status.dwWaitHint / 10;

            if (dwWaitTime < 1000)
                dwWaitTime = 1000;
            else if (dwWaitTime > 10000)
                dwWaitTime = 10000;

            Thread.Sleep(dwWaitTime);

            // Check the status again.

            if (WinApi.QueryServiceStatus(service, status) == 0)
                break;

            if (status.dwCheckPoint > dwOldCheckPoint)
            {
                // The service is making progress.
                dwStartTickCount = Environment.TickCount;
                dwOldCheckPoint = status.dwCheckPoint;
            }
            else
            {
                if (Environment.TickCount - dwStartTickCount > status.dwWaitHint)
                {
                    // No progress made within the wait hint
                    break;
                }
            }
        }

        return (status.dwCurrentState == desiredStatus);
    }

    public static void Uninstall(string serviceName)
    {
        var scm = OpenSCManager(WinApi.ScmAccessRights.AllAccess);

        try
        {
            var service = WinApi.OpenService(scm, serviceName, WinApi.ServiceAccessRights.AllAccess);
            if (service == IntPtr.Zero)
                throw new ApplicationException("Service not installed.");

            try
            {
                StopService(service);
                if (!WinApi.DeleteService(service))
                    throw new ApplicationException("Could not delete service " + Marshal.GetLastWin32Error());
            }
            finally
            {
                WinApi.CloseServiceHandle(service);
            }
        }
        finally
        {
            WinApi.CloseServiceHandle(scm);
        }
    }
}
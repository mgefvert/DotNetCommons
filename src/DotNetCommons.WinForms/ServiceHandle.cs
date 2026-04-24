using System.ComponentModel;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace DotNetCommons.WinForms;

public class ServiceHandle : IDisposable
{
    public WinApi.QUERY_SERVICE_CONFIG Config { get; }
    public string Name { get; }
    private readonly IntPtr _handle;

    public ServiceHandle(IntPtr serviceHandle, string name)
    {
        Name = name;
        _handle = serviceHandle;
        Config = GetConfig();
    }

    private void ReleaseUnmanagedResources()
    {
        WinApi.CloseServiceHandle(_handle);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ServiceHandle()
    {
        ReleaseUnmanagedResources();
    }

    public WinApi.QUERY_SERVICE_CONFIG GetConfig()
    {
        // First call: ask for buffer size
        WinApi.QueryServiceConfig(_handle, IntPtr.Zero, 0, out int bytesNeeded);
        if (bytesNeeded == 0)
            throw new Win32Exception();

        var buffer = Marshal.AllocHGlobal(bytesNeeded);
        try
        {
            return WinApi.QueryServiceConfig(_handle, buffer, bytesNeeded, out _) 
                ? Marshal.PtrToStructure<WinApi.QUERY_SERVICE_CONFIG>(buffer) 
                : throw new Win32Exception();
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
    
    public WinApi.SERVICE_STATUS GetServiceStatus()
    {
        var status = new WinApi.SERVICE_STATUS();
        return WinApi.QueryServiceStatus(_handle, ref status) != 0
            ? status
            : throw new Win32Exception();
    }

    public void Start()
    {
        var resultCode = WinApi.StartService(_handle, 0, 0);
        if (resultCode == 0)
            throw new Win32Exception();
        
        var changedStatus = WaitForServiceStatus(WinApi.ServiceState.StartPending, WinApi.ServiceState.Running);
        if (!changedStatus)
            throw new ApplicationException("Unable to start service");
    }

    public void Stop()
    {
        var status = new WinApi.SERVICE_STATUS();
        var resultCode = WinApi.ControlService(_handle, WinApi.ServiceControl.Stop, status);
        if (resultCode == 0)
            throw new Win32Exception();
        
        var changedStatus = WaitForServiceStatus(WinApi.ServiceState.StopPending, WinApi.ServiceState.Stopped);
        if (!changedStatus)
            throw new ApplicationException("Unable to stop service");
    }

    public bool WaitForServiceStatus(WinApi.ServiceState waitStatus, WinApi.ServiceState desiredStatus)
    {
        var status = new WinApi.SERVICE_STATUS();

        WinApi.QueryServiceStatus(_handle, ref status);
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

            if (WinApi.QueryServiceStatus(_handle, ref status) == 0)
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

        return status.dwCurrentState == desiredStatus;
    }

    public void Uninstall()
    {
        if (!WinApi.DeleteService(_handle))
            throw new Win32Exception();
    }
}
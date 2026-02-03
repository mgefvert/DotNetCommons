using System.ComponentModel;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace DotNetCommons.WinForms;

public class ServiceControlManager : IDisposable
{
    private readonly IntPtr _scm;
    
    public ServiceControlManager(WinApi.ScmAccessRights rights = WinApi.ScmAccessRights.Connect)
    {
        _scm = WinApi.OpenSCManager(null, null, rights);
        if (_scm == IntPtr.Zero)
            throw new Win32Exception();
    }

    private void ReleaseUnmanagedResources()
    {
        WinApi.CloseServiceHandle(_scm);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ServiceControlManager()
    {
        ReleaseUnmanagedResources();
    }
    
    public ServiceHandle OpenService(string serviceName)
    {
        return TryOpenService(serviceName) ?? throw new Win32Exception();
    }

    public ServiceHandle? TryOpenService(string serviceName, WinApi.ServiceAccessRights accessRights = WinApi.ServiceAccessRights.StandardRightsRequired)
    {
        var handle = WinApi.OpenService(_scm, serviceName, accessRights);
        return handle != IntPtr.Zero 
            ? new ServiceHandle(handle, serviceName)
            : null;
    }

    public ServiceHandle InstallAndOpen(string serviceName, string displayName, string fileName, WinApi.ServiceBootFlag startFlag = WinApi.ServiceBootFlag.AutoStart)
    {
        var handle = WinApi.CreateService(_scm, serviceName, displayName, WinApi.ServiceAccessRights.AllAccess, WinApi.SERVICE_WIN32_OWN_PROCESS,
                startFlag, WinApi.ServiceError.Normal, fileName, null, IntPtr.Zero, null, null, null);
        
        return handle != IntPtr.Zero 
            ? new ServiceHandle(handle, serviceName) 
            : throw new Win32Exception();
    }

    public bool IsInstalled(string serviceName)
    {
        var service = WinApi.OpenService(_scm, serviceName, WinApi.ServiceAccessRights.QueryStatus);
        if (service == IntPtr.Zero)
            return false;

        WinApi.CloseServiceHandle(service);
        return true;
    }
}
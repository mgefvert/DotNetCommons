using System.Reflection;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms;

public static class ControlExtensions
{
    public static void SetDoubleBuffered(this Control control, bool enable)
    {
        var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
        doubleBufferPropertyInfo?.SetValue(control, enable, null);
    }
}
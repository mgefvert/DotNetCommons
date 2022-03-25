using System;
using System.Text;

namespace DotNetCommons;

public static class ExceptionExtensions
{
    public static string GetDetailedInformation(this Exception ex, bool stackTrace)
    {
        var sb = new StringBuilder();

        sb.AppendLine(ex.GetType().Name + ": " + ex.Message);

        var inner = ex.InnerException;
        while (inner != null)
        {
            sb.AppendLine(" > " + inner.GetType().Name + ": " + inner.Message);
            inner = inner.InnerException;
        }

        if (!string.IsNullOrEmpty(ex.Source))
            sb.AppendLine("Source: " + ex.Source);

        if (ex.TargetSite != null)
            sb.AppendLine("TargetSite: " + ex.TargetSite);

        if (stackTrace)
        {
            sb.AppendLine("Stack trace:");
            sb.AppendLine(ex.StackTrace);
        }

        return sb.ToString();
    }
}
using System.Net;
using Microsoft.AspNetCore.Http;

namespace DotNetCommons.Web;

public static class HttpContextExtensions
{
    public static IPAddress GetRemoteIpAddress(this HttpContext context)
    {
        var value = (string?)context.Request.Headers["X-Real-IP"];
        if (value.IsSet() && IPAddress.TryParse(value, out var ip))
            return ip;

        value = (string?)context.Request.Headers["X-Forwarded-For"];
        if (value.IsSet() && IPAddress.TryParse(value.GetSubItem(',', 0), out ip))
            return ip;
        
        return context.Connection.RemoteIpAddress!;
    }
}
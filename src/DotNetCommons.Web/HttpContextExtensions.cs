using System.Net;
using Microsoft.AspNetCore.Http;

namespace DotNetCommons.Web;

public static class HttpContextExtensions
{
    /// <summary>
    /// Retrieves the remote IP address from the HttpContext, taking into account various proxy headers.
    /// </summary>
    /// <param name="context">The HttpContext instance that provides access to the HTTP request and connection details.</param>
    /// <param name="trustProxyHeaders">Whether to trust proxy headers (should only be true if behind a known reverse proxy).</param>
    /// <returns>The remote IP address of the client making the request, or null if no valid IP could be determined.</returns>
    public static IPAddress? GetRemoteIpAddress(this HttpContext context, bool trustProxyHeaders = false)
    {
        // Only trust proxy headers if explicitly enabled
        if (!trustProxyHeaders)
            return context.Connection.RemoteIpAddress;

        // Try RFC 7239 Forwarded header
        var forwarded = context.Request.Headers["Forwarded"].ToString();
        if (!string.IsNullOrEmpty(forwarded))
        {
            var forwardedFor = forwarded
                .Split([';', ',', ':'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault(p => p.StartsWith("for=", StringComparison.OrdinalIgnoreCase));

            if (forwardedFor != null)
            {
                var ip = forwardedFor["for=".Length..].Trim('"', '[', ']');
                if (IPAddress.TryParse(ip, out var parsedIp))
                    return parsedIp;
            }
        }

        // Try X-Forwarded-For
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            // Get the first IP in the chain (original client)
            var ips = xForwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (ips.Length > 0 && IPAddress.TryParse(ips[0], out var parsedIp))
                return parsedIp;
        }

        // Try X-Real-IP
        var xRealIp = context.Request.Headers["X-Real-IP"].ToString();
        if (!string.IsNullOrEmpty(xRealIp) && IPAddress.TryParse(xRealIp.Trim(), out var parsedXRealIp))
            return parsedXRealIp;

        // Fall back to connection remote address
        return context.Connection.RemoteIpAddress;
    }
}
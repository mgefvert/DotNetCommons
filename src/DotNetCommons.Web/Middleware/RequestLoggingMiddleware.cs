using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotNetCommons.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path   = context.Request.Path;

        _logger.LogDebug("HTTP {method} {path}{query}", method, path, context.Request.QueryString);

        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        _logger.LogDebug("HTTP response -> {milliseconds}ms: {status} type={type} len={len}",
            sw.ElapsedMilliseconds, context.Response.StatusCode, context.Response.ContentType, context.Response.ContentLength);
    }
}

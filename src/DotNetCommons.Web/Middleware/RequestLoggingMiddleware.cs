using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotNetCommons.Web.Middleware;

/// <summary>
/// Middleware that just prints requests on screen.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<HttpStatusException> logger)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var t0 = DateTime.UtcNow;
            await _next(context);
            Console.WriteLine("{0,3} {1,4} ms {2,5} kB {3,-4} {4}", context.Response.StatusCode, (int)(DateTime.UtcNow - t0).TotalMilliseconds,
                context.Response.ContentLength / 1024, context.Request.Method, context.Request.Path);
        }
        catch (Exception e)
        {
            Console.WriteLine($"{context.Request.Method} {context.Request.Path}: {e.Message}");
        }
    }
}
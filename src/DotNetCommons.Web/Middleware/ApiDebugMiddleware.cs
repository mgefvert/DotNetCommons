using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNetCommons.Web.Middleware;

public class ApiDebugMiddleware
{
    private readonly RequestDelegate _next;

    public ApiDebugMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var t0 = DateTime.UtcNow;
        try
        {
            await _next(context);
            Console.WriteLine($"{context.Request.Method} {context.Request.Path} ... {context.Response.StatusCode} in {(DateTime.UtcNow - t0).TotalMilliseconds:N0}ms");
        }
        catch (Exception e)
        {
            Console.WriteLine($"{context.Request.Method} {context.Request.Path} ... {e.GetType().Name}: '{e.Message}' in {(DateTime.UtcNow - t0).TotalMilliseconds:N0}ms");
        }
    }
}
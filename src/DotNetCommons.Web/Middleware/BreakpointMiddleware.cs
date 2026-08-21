using Microsoft.AspNetCore.Http;

namespace DotNetCommons.Web.Middleware;

/// The purpose of this middleware is to simply have a place for a nice breakpoint just before the endpoint handler is being executed.
/// By this time all the authorization, routing and everything else has been done; and custom handlers start processing.
public class BreakpointMiddleware
{
    private readonly RequestDelegate _next;

    public BreakpointMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        return _next(context);
    }
}

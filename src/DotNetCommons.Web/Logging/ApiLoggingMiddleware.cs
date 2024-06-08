using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetCommons.Web.Logging;

/// <summary>
/// Middleware that catches unhandled exceptions and returns ProblemDetails JSON data.
/// </summary>
public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ApiLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
        var logRequest = context.RequestServices.GetRequiredService<ApiLogRequest>();
        
        var controller = context.GetRouteData().Values["controller"]?.ToString();
        var action     = context.GetRouteData().Values["action"]?.ToString();
        var logger     = loggerFactory.CreateLogger(controller ?? "Unknown");
        try
        {
            await _next(context);

            var elapsed = logRequest.Elapsed;
            var request = logRequest.RequestToString();
            var data = logRequest.DataToString();
           
            if (!string.IsNullOrEmpty(data))
                logger.LogInformation("{controller}.{action}{request} => {status} in {time}ms: {data}",
                    controller, action, request, context.Response.StatusCode, elapsed, data);
            else
                logger.LogInformation("{controller}.{action}{request} => {status} in {time}ms",
                    controller, action, request, context.Response.StatusCode, elapsed);
        }
        catch (AppException ex)
        {
            logger.LogWarning("{controller}.{action} => {status} in {time}ms: {message}",
                controller, action, (int)ex.StatusCode, logRequest.Elapsed, ex.Message);
            await HandleError(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError("{controller}.{action}: Unhandled {exception} after {time}ms: {message}",
                controller, action, ex.GetType().Name, logRequest.Elapsed, ex.Message);
            await HandleError(context, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private static async Task HandleError(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(message);
    }
}
using System.Net;
using Microsoft.AspNetCore.Http;
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
        var logRequest    = context.RequestServices.GetRequiredService<ApiLogRequest>();
        
        var logger = loggerFactory.CreateLogger(nameof(ApiLoggingMiddleware));
        try
        {

            await _next(context);

            var elapsed = logRequest.Elapsed;
            var request = logRequest.RequestToString();
            var data = logRequest.DataToString();
           
            if (!string.IsNullOrEmpty(data))
                logger.LogInformation("{path}{request} => {status} in {time}ms: {data}",
                    context.Request.Path, request, context.Response.StatusCode, elapsed, data);
            else
                logger.LogInformation("{path}{request} => {status} in {time}ms",
                    context.Request.Path, request, context.Response.StatusCode, elapsed);
        }
        catch (AppException ex)
        {
            logger.LogWarning("{path} => {status} in {time}ms: {message}",
                context.Request.Path, (int)ex.StatusCode, logRequest.Elapsed, ex.Message);
            await HandleError(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError("{path}: Unhandled {exception} after {time}ms: {message}",
                context.Request.Path, ex.GetType().Name, logRequest.Elapsed, ex.Message);
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
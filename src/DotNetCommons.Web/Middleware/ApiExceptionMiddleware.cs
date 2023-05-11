using System;
using System.Net;
using System.Threading.Tasks;
using DotNetCommons.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetCommons.Web.Middleware;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<AppException> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException e)
        {
            _logger.LogInformation("{Exception}: HTTP/{StatusCode} '{Message}' in {Method} {Path}",
                e.GetType().Name, (int)e.StatusCode, e.Message, context.Request.Method, context.Request.Path);
            await HandleError(context, e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unhandled exception occurred in {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await HandleError(context, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private async Task HandleError(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = ContentTypes.Json;
        await context.Response.WriteAsync(JsonConvert.SerializeObject(new ProblemDetails
        {
            Title = message,
            Status = (int)statusCode
        }));
    }
}
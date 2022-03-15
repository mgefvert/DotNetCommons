using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNetCommons.Web;

public class ApiErrorCatchingMiddleware
{
    private Stream Message(string msg)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(msg));
    }

    public async Task Middleware(HttpContext context, Func<Task> next)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next();
            return;
        }

        try
        {
            await next();
        }
        catch (ApiException e)
        {
            context.Response.StatusCode = (int)e.HttpCode;
            context.Response.Body = Message(e.Message);
        }
        catch
        {
            context.Response.StatusCode = 500;
            context.Response.Body = Message("Server Internal Error");
        }
    }
}
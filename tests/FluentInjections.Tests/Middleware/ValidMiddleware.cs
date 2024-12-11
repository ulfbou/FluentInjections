using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Middleware;

// Valid and Invalid Middleware Classes
public class ValidMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        return next(context);
    }
}

using Xunit;
using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Middleware;

public class ConstructorMiddleware : IMiddleware
{
    public ConstructorMiddleware(int value, string text) { }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        return next(context);
    }
}

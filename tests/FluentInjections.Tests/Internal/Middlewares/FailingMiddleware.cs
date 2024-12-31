using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Middlewares;

internal class FailingMiddleware(RequestDelegate next, ILogger<FailingMiddleware> logger) : BaseMiddleware(next, logger)
{
    public override async Task Invoke(HttpContext context)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An error occurred.");
    }
}

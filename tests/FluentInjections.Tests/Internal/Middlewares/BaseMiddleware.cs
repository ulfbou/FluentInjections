using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Middlewares;

internal abstract class BaseMiddleware
{
    protected readonly RequestDelegate _next;
    protected readonly ILogger _logger;

    public BaseMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public abstract Task Invoke(HttpContext context);
}

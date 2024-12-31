using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Middlewares;

internal class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        _logger.LogInformation("Handling request: " + context.Request.Path);
        await _next(context);
        _logger.LogInformation("Finished handling request.");
    }
}

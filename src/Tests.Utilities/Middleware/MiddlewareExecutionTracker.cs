using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Utilities;

public class MiddlewareExecutionTracker
{
    private readonly List<string> _executedMiddleware = new();

    public IReadOnlyList<string> ExecutedMiddleware => _executedMiddleware.AsReadOnly();

    public RequestDelegate WrapMiddleware(RequestDelegate middleware, string middlewareName)
    {
        return async context =>
        {
            _executedMiddleware.Add(middlewareName);
            await middleware(context);
        };
    }
}

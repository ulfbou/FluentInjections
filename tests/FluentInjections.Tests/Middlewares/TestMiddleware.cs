using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Middlewares;

internal class TestMiddleware
{
    public static int id = 0;
    public static List<int> CallOrder { get; } = new();

    public int Id { get; set; }

    public TestMiddleware() => Id = ++id;

    public virtual Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        CallOrder.Add(Id);
        return Task.CompletedTask;
    }
}
internal class MiddlewareA : TestMiddleware, IMiddleware { }
internal class MiddlewareB : TestMiddleware, IMiddleware { }
internal class MiddlewareC : TestMiddleware, IMiddleware { }

internal class TestMiddleware<TOptions> : TestMiddleware where TOptions : class
{
    public static TOptions LastOptions = default!;
    private readonly TOptions _options;
    private readonly RequestDelegate _next;
    public TestMiddleware(RequestDelegate next, TOptions options) : base()
    {
        _next = next;
        _options = options;
        LastOptions = options;
    }

    public override async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        CallOrder.Add(1);
        LastOptions = _options;
        await _next(context);
    }
}
public class TestOptions
{
    public string Value { get; set; }
}

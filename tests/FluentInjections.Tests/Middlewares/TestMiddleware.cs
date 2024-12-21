using Microsoft.AspNetCore.Http;

using System.Diagnostics;

namespace FluentInjections.Tests.Middlewares;

internal class TestMiddlewareBase
{
    protected readonly List<Type> _pipelineOrder;

    public TestMiddlewareBase(List<Type> pipelineOrder)
    {
        _pipelineOrder = pipelineOrder;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Debug.WriteLine($"Invoking {this.GetType().Name}");
        _pipelineOrder.Add(this.GetType());
        await next(context);
    }
}

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

internal class MiddlewareA(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareB(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareC(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareD(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareE(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
internal class MiddlewareF(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }

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

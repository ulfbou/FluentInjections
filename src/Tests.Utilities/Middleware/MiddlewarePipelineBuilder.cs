using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Reflection;

namespace FluentInjections.Tests.Utilities;

/// <inheritdoc />
public class MiddlewarePipelineBuilder : IMiddlewarePipelineBuilder
{
    private readonly List<(Type MiddlewareType, object?[]? Args)> _middlewareDescriptors = new();
    private readonly IServiceCollection _services;
    private IServiceProvider? _serviceProvider;
    private readonly ILogger<MiddlewarePipelineBuilder> _logger;

    public MiddlewarePipelineBuilder(IServiceCollection services, ILogger<MiddlewarePipelineBuilder> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IMiddlewarePipelineBuilder UseMiddleware<TMiddleware>(params object?[]? args) where TMiddleware : class, IMiddleware
    {
        _middlewareDescriptors.Add((typeof(TMiddleware), args ?? Array.Empty<object>()));
        return this;
    }

    /// <inheritdoc />
    public IMiddlewarePipelineBuilder UseMiddleware(Type middlewareType, params object?[]? args)
    {
        _middlewareDescriptors.Add((middlewareType, args ?? Array.Empty<object>()));
        return this;
    }

    /// <inheritdoc />
    public RequestDelegate Build()
    {
        _serviceProvider = _services.BuildServiceProvider();

        RequestDelegate next = context => Task.CompletedTask;

        foreach (var (middlewareType, args) in _middlewareDescriptors.AsEnumerable().Reverse())
        {
            var middlewareInstance = CreateMiddlewareInstance(middlewareType, args!);
            next = BuildMiddlewareDelegate(middlewareInstance, next);
        }

        return next;
    }

    private IMiddleware CreateMiddlewareInstance(Type middlewareType, object[] args)
    {
        var constructor = middlewareType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Array.ConvertAll(args, arg => arg?.GetType() ?? typeof(object)), null);
        if (constructor is null)
        {
            throw new InvalidOperationException($"No suitable constructor found for middleware type {middlewareType.Name}");
        }

        return (IMiddleware)constructor.Invoke(args);
    }

    private RequestDelegate BuildMiddlewareDelegate(IMiddleware middleware, RequestDelegate next)
    {
        return async context =>
        {
            await middleware.InvokeAsync(context, next);
        };
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Reflection;

namespace FluentInjections.Tests.Utilities;

/// <summary>
/// Builds a middleware pipeline. This class is used to configure and build a pipeline of middleware components.
/// </summary>
/// <remarks>
/// This class is used to configure and build a pipeline of middleware components. The purpose of this class is to provide a fluent API for building middleware pipelines, which can be used to test middleware components in isolation.
/// </remarks>
public class MiddlewarePipelineBuilder : IMiddlewarePipelineBuilder
{
    private readonly List<Func<RequestDelegate, RequestDelegate>> _middlewares = new List<Func<RequestDelegate, RequestDelegate>>();
    private readonly IServiceCollection _services;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MiddlewarePipelineBuilder> _logger;

    public MiddlewarePipelineBuilder(IServiceCollection services, ILogger<MiddlewarePipelineBuilder> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = _services.BuildServiceProvider();
    }

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware component to add to the pipeline.</typeparam>
    /// <param name="args">The arguments to pass to the middleware component's constructor.</param>
    /// <returns>The current instance of the <see cref="IMiddlewarePipelineBuilder"/> interface.</returns>
    public IMiddlewarePipelineBuilder UseMiddleware<TMiddleware>(params object?[]? args) where TMiddleware : class, IMiddleware
    {
        if (args == null) throw new ArgumentNullException(nameof(args));

        Type middlewareType = typeof(TMiddleware);

        _logger.LogDebug("Adding middleware component {MiddlewareType} to the pipeline.", middlewareType.Name);

        _services.AddTransient<TMiddleware>(provider =>
        {
            var constructors = middlewareType.GetConstructors().Where(ctor => ctor.GetParameters().Length == args.Length).ToList();

            if (constructors.Count == 0)
            {
                throw new InvalidOperationException($"Middleware {middlewareType.Name} does not have a public constructor with {args.Length} parameters.");
            }

            foreach (var ctor in constructors)
            {
                if (TryCreateFromConstructor(middlewareType, ctor, args, out var instance))
                {
                    return (TMiddleware)instance!;
                }
            }

            throw new InvalidOperationException($"Could not create an instance of middleware {middlewareType.Name} with the provided arguments.");
        });

        _middlewares.Add(next => async context =>
        {
            var middleware = _serviceProvider.GetRequiredService<TMiddleware>();
            await middleware.InvokeAsync(context, next);
        });

        return this;
    }

    private bool TryCreateFromConstructor(Type type, ConstructorInfo ctor, object?[] args, out object? instance)
    {
        var parameters = ctor.GetParameters();
        var ctorArgs = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            ctorArgs[i] = args[i];
        }

        try
        {
            instance = Activator.CreateInstance(type, ctorArgs);
            return true;
        }
        catch
        {
            instance = null;
            return false;
        }
    }

    /// <summary>
    /// Builds the middleware pipeline, and activates the pipeline components.
    /// </summary>
    /// <returns>A <see cref="RequestDelegate"/> that represents the middleware pipeline.</returns>
    public RequestDelegate Build()
    {
        RequestDelegate next = context => Task.CompletedTask;

        foreach (var middleware in _middlewares.AsEnumerable().Reverse())
        {
            next = middleware(next);
        }

        return next;
    }
}

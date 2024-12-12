using System.Net.Http;

using Autofac;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentInjections;

/// <summary>
/// Represents a middleware configurator.
/// </summary>
public sealed class MiddlewareConfigurator<TBuilder> : IMiddlewareConfigurator<TBuilder>
{
    public TBuilder Builder => _builder;
    private TBuilder _builder;
    private readonly IServiceProvider _serviceProvider;

    public MiddlewareConfigurator(TBuilder builder, IServiceProvider serviceProvider)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        if (typeof(TBuilder) != builder.GetType() && !typeof(TBuilder).IsAssignableFrom(builder.GetType()))
        {
            throw new ArgumentException($"Builder type {builder.GetType().Name} is not assignable to {typeof(TBuilder).Name}.");
        }

        if (!IsBuilderTypeValid(builder))
        {
            throw new NotSupportedException($"Builder type {builder.GetType().Name} is not supported.");
        }
    }

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type.</typeparam>
    /// <param name="args">The optional arguments to pass to the middleware constructor.</param>
    /// <returns>The middleware configurator.</returns>
    public IMiddlewareConfigurator<TBuilder> Use<TMiddleware>(params object?[] args) where TMiddleware : class, IMiddleware
    {
        var middlewareType = typeof(TMiddleware);

        if (!IsMiddlewareTypeValid(typeof(TMiddleware)))
        {
            throw new ArgumentException($"The middleware type {middlewareType.Name} does not implement IMiddleware.");
        }

        // Create middleware with args
        TMiddleware middleware = ActivatorUtilities.CreateInstance<TMiddleware>(_serviceProvider, (args ?? Array.Empty<object>()) as object[]);

        RequestDelegate requestDelegate = context => middleware.InvokeAsync(context, _ => Task.CompletedTask);

        if (Builder is IApplicationBuilder app)
        {
            app.Use(next => requestDelegate);
        }
        else
        {
            throw new NotSupportedException($"Builder type {Builder!.GetType().Name} is not supported.");
        }

        return this;
    }

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <param name="middlewareType">The middleware type.</param>
    /// <param name="args">The optional arguments to pass to the middleware constructor.</param>
    /// <returns>The middleware configurator.</returns>
    public IMiddlewareConfigurator<TBuilder> Use(Type middlewareType, params object?[] args)
    {
        if (!IsMiddlewareTypeValid(middlewareType))
        {
            //throw new ArgumentException($"The middleware type {middlewareType.Name} does not implement IMiddleware.");
        }

        if (Builder is IApplicationBuilder app)
        {
            app.UseMiddleware(middlewareType, args);
        }
        else if (Builder is WebApplication webApp)
        {
            webApp.UseMiddleware(middlewareType, args);
        }
        else
        {
            throw new InvalidOperationException($"Builder type {Builder!.GetType().Name} is not supported. We should never ever wind up here.");
            //throw new NotSupportedException("$"Builder type {Builder!.GetType().Name} is not supported.");
        }

        return this;
    }

    private bool IsBuilderTypeValid(TBuilder builder) => builder is IApplicationBuilder || builder is IMiddlewarePipelineBuilder;

    private bool IsMiddlewareTypeValid(Type middlewareType) =>
        (typeof(IMiddleware).IsAssignableFrom(middlewareType) ||
        typeof(IMiddlewarePipelineBuilder).IsAssignableFrom(middlewareType)) && !middlewareType.IsAbstract && !middlewareType.IsInterface;
}

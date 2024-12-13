using Microsoft.AspNetCore.Http;

namespace FluentInjections;

/// <summary>
/// Builds a middleware pipeline. This class is used to configure and build a pipeline of middleware components.
/// </summary>
/// <remarks>
/// This class is used to configure and build a pipeline of middleware components. The purpose of this class is to provide a fluent API for building middleware pipelines, which can be used to test middleware components in isolation.
/// </remarks>
public interface IMiddlewarePipelineBuilder
{
    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware component to add to the pipeline.</typeparam>
    /// <param name="args">The arguments to pass to the middleware component's constructor.</param>
    /// <returns>The current instance of the <see cref="IMiddlewarePipelineBuilder"/> interface.</returns>
    IMiddlewarePipelineBuilder UseMiddleware<TMiddleware>(params object?[] objects) where TMiddleware : class, IMiddleware;

    /// <summary>
    /// Adds a middleware component to the pipeline.
    /// </summary>
    /// <param name="middlewareType">The type of the middleware component to add to the pipeline.</param>
    /// <param name="args">The arguments to pass to the middleware component's constructor.</param>
    /// <returns>The current instance of the <see cref="IMiddlewarePipelineBuilder"/> interface.</returns>
    IMiddlewarePipelineBuilder UseMiddleware(Type middlewareType, params object?[]? args);

    /// <summary>
    /// Builds the middleware pipeline, and activates the pipeline components.
    /// </summary>
    /// <returns>A <see cref="RequestDelegate"/> that represents the middleware pipeline.</returns>
    RequestDelegate Build();
}
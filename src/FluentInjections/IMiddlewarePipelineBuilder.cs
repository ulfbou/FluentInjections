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
    RequestDelegate Build();
    IMiddlewarePipelineBuilder UseMiddleware<TMiddleware>(params object?[] objects) where TMiddleware : class, IMiddleware;
}
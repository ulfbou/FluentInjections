using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Extensions;

public static class MiddlewarePipelineHelperExtensions
{
    public static RequestDelegate CreatePipeline(this IEnumerable<IMiddleware> middlewares, RequestDelegate finalHandler)
    {
        return middlewares.Reverse().Aggregate(finalHandler, (next, middleware) =>
            context => middleware.InvokeAsync(context, next));
    }

    /// <summary>
    /// Visualizes the pipeline.
    /// </summary>
    /// <param name="middlewares">The middlewares.</param>
    /// <returns>A string representation of the pipeline.</returns>
    public static string VisualizePipeline(this IEnumerable<IMiddleware> middlewares) => string.Join(" -> ", middlewares.Select(middleware => middleware.GetType().Name));

    /// <summary>
    /// Adds a middleware to the pipeline conditionally.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type.</typeparam>
    /// <param name="middlewares">The middlewares.</param>
    /// <param name="condition">The condition to add the middleware.</param>
    /// <returns>The updated middlewares.</returns>
    public static IEnumerable<IMiddleware> AddMiddlewareIf<TMiddleware>(
        this IEnumerable<IMiddleware> middlewares,
        Func<bool> condition,
        Func<IMiddleware> middlewareFactory)
    {
        return condition() ? middlewares.Append(middlewareFactory()) : middlewares;
    }

    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type.</typeparam>
    /// <param name="middlewares">The middlewares.</param>
    /// <returns>The updated middlewares.</returns>
    public static IEnumerable<IMiddleware> RemoveMiddleware<TMiddleware>(this IEnumerable<IMiddleware> middlewares)
    {
        return middlewares.Where(middleware => middleware.GetType() != typeof(TMiddleware));
    }

    /// <summary>
    /// Replaces a middleware in the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type.</typeparam>
    /// <param name="middlewares">The middlewares.</param>
    /// <param name="replacement">The replacement middleware.</param>
    /// <returns>The updated middlewares.</returns>
    public static IEnumerable<IMiddleware> ReplaceMiddleware<TMiddleware>(
        this IEnumerable<IMiddleware> middlewares,
        IMiddleware replacement)
    {
        return middlewares.Select(middleware => middleware.GetType() == typeof(TMiddleware)
            ? replacement
            : middleware);
    }

    /// <summary>
    /// Wraps a middleware in the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type.</typeparam>
    /// <param name="middlewares">The middlewares.</param>
    /// <param name="wrapper">The wrapper middleware.</param>
    public static IEnumerable<IMiddleware> WrapMiddleware<TMiddleware>(
        this IEnumerable<IMiddleware> middlewares,
        Func<IMiddleware, IMiddleware> wrapper)
    {
        return middlewares.Select(middleware => middleware.GetType() == typeof(TMiddleware)
            ? wrapper(middleware)
            : middleware);
    }

    /// <summary>
    /// Simulates the execution of the pipeline.
    /// </summary>
    /// <param name="middlewares">The middlewares.</param>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SimulateExecutionAsync(
        this IEnumerable<IMiddleware> middlewares,
        HttpContext context)
    {
        RequestDelegate finalHandler = _ => Task.CompletedTask;
        RequestDelegate pipeline = middlewares.CreatePipeline(finalHandler);
        await pipeline(context);
    }
}

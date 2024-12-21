using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FluentInjections;

/// <summary>
/// Provides functionality to configure middleware for the application.
/// </summary>
/// <typeparam name="TBuilder">The type of the application.</typeparam>
public interface IMiddlewareConfigurator<TBuilder> where TBuilder : class
{
    /// <summary>
    /// Gets the application instance.
    /// </summary>
    TBuilder Builder { get; }

    /// <summary>
    /// Registers a middleware of the specified type.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to register.</typeparam>
    /// <returns>A binding interface to configure the middleware.</returns>
    IMiddlewareBinding UseMiddleware<TMiddleware>() where TMiddleware : class;

    /// <summary>
    /// Registers a middleware of the specified type.
    /// </summary>
    /// <param name="middleware">The type of the middleware to register.</typeparam>
    /// <returns>A binding interface to configure the middleware.</returns>
    IMiddlewareBinding UseMiddleware(Type middleware);

    /// <summary>
    /// Removes a middleware of the specified type.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to remove.</typeparam>
    /// <returns>A binding interface to configure the middleware removal.</returns>
    IMiddlewareBinding RemoveMiddleware<TMiddleware>() where TMiddleware : class;

    /// <summary>
    /// Removes a middleware of the specified type.
    /// </summary>
    /// <param name="middleware">The type of the middleware to remove.</typeparam>
    /// <returns>A binding interface to configure the middleware removal.</returns>
    IMiddlewareBinding RemoveMiddleware(Type middleware);

    /// <summary>
    /// Gets the configuration binding for a middleware of the specified type.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to get.</typeparam>
    /// <returns>A binding interface to configure the middleware.</returns>
    IMiddlewareBinding GetMiddleware<TMiddleware>() where TMiddleware : class;

    /// <summary>
    /// Gets the configuration binding for a middleware of the specified type.
    /// </summary>
    /// <param name="middleware">The type of the middleware to get.</typeparam>
    /// <returns>A binding interface to configure the middleware.</returns>
    IMiddlewareBinding GetMiddleware(Type middleware);

    /// <summary>
    /// Applies a configuration action to all middleware in the specified group.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    /// <param name="configure">The action to configure the middleware.</param>
    void ApplyGroupPolicy(string groupName, Action<IMiddlewareBinding> configure);

    /// <summary>
    /// Applies a configuration action to all middleware.
    /// </summary>
    /// <param name="configure">The action to configure the middleware.</param>
    void ConfigureAll(Action<IMiddlewareBinding> configure);

    /// <summary>
    /// Registers the middleware configuration with the application.
    /// </summary>
    void Register();
}

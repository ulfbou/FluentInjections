using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FluentInjections;

/// <summary>
/// Provides functionality to configure middleware for the application.
/// </summary>
/// <typeparam name="TApplication">The type of the application.</typeparam>
public interface IMiddlewareConfigurator<TApplication> where TApplication : class
{
    /// <summary>
    /// Gets the application instance.
    /// </summary>
    TApplication Application { get; }

    /// <summary>
    /// Registers a middleware of the specified type.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to register.</typeparam>
    /// <returns>A binding interface to configure the middleware.</returns>
    IMiddlewareBinding<TMiddleware, TApplication> UseMiddleware<TMiddleware>() where TMiddleware : class;

    /// <summary>
    /// Removes a middleware of the specified type.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to remove.</typeparam>
    /// <returns>A binding interface to configure the middleware removal.</returns>
    IMiddlewareBinding<TMiddleware, TApplication> RemoveMiddleware<TMiddleware>() where TMiddleware : class;

    /// <summary>
    /// Gets the configuration binding for a middleware of the specified type.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to get.</typeparam>
    /// <returns>A binding interface to configure the middleware.</returns>
    IMiddlewareBinding<TMiddleware, TApplication> GetMiddleware<TMiddleware>() where TMiddleware : class;

    /// <summary>
    /// Applies a configuration action to all middleware in the specified group.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware in the group.</typeparam>
    /// <param name="groupName">The name of the group.</param>
    /// <param name="configure">The action to configure the middleware.</param>
    void ApplyGroupPolicy<TMiddleware>(string groupName, Action<IMiddlewareBinding<TMiddleware, TApplication>> configure) where TMiddleware : class;

    /// <summary>
    /// Applies a configuration action to all middleware.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware.</typeparam>
    /// <param name="configure">The action to configure the middleware.</param>
    void ConfigureAll<TMiddleware>(Action<IMiddlewareBinding<TMiddleware, TApplication>> configure) where TMiddleware : class;
}

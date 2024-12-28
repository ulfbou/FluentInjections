// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FluentInjections;

/// <summary>
/// Represents a middleware configurator that provides methods to configure middleware components within the application.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define middleware configurations.
/// </remarks>
public interface IMiddlewareConfigurator : IConfigurator<IMiddlewareBinding>
{
    /// <summary>
    /// Gets the application instance.
    /// </summary>
    object Middleware { get; }

    /// <summary>
    /// Gets the type of the application builder.
    /// </summary>
    Type MiddlewareType { get; }

    /// <summary>
    /// Registers a middleware of the specified type.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to register.</typeparam>
    /// <returns>A binding interface to configure the middleware.</returns>
    IMiddlewareBinding<TMiddleware> UseMiddleware<TMiddleware>() where TMiddleware : class;


    /// <summary>
    /// Removes a middleware of the specified type.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to remove.</typeparam>
    /// <param name="descriptor">Optional. The descriptor of the middleware to remove.</param>
    /// <returns><see langword="true" />if the middleware was removed; otherwise, <see langword="false" />.</returns>
    bool RemoveMiddleware<TMiddleware>(MiddlewareBindingDescriptor? descriptor = null) where TMiddleware : class;

    /// <summary>
    /// Gets the configuration binding for a middleware of the specified type.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware to get.</typeparam>
    /// <param name="descriptor">Optional. The descriptor of the middleware to get.</param>
    /// <returns>A binding interface to configure the middleware.</returns>
    IMiddlewareBinding<TMiddleware>? GetMiddleware<TMiddleware>(MiddlewareBindingDescriptor? descriptor = null) where TMiddleware : class;

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
}

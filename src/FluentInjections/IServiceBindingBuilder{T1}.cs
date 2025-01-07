// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

/// <summary>
/// Represents a service binding that provides methods to bind and configure services within the application.
/// </summary>
/// <typeparam name="TService">The type of the service.</typeparam>
/// <remarks>
/// This interface should be implemented by classes that define service bindings.
/// </remarks>
public interface IServiceBindingBuilder<TService> : IServiceBindingBuilder where TService : notnull
{
    /// <summary>
    /// Binds the service to a specific implementation type.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder<TService> To<TImplementation>() where TImplementation : class, TService;

    /// <summary>
    /// Uses a factory method to create the service instance.
    /// </summary>
    /// <param name="factory">The factory method.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder<TService> WithFactory(Func<IServiceProvider, TService> factory);

    /// <summary>
    /// Uses a specific instance for the service.
    /// </summary>
    /// <param name="instance">The service instance.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder<TService> WithInstance(TService instance);

    /// <summary>
    /// Configures the service after it is created.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder<TService> Configure(Action<TService> configure);

    /// <summary>
    /// Configures options for the service.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    /// <param name="configure">The options configuration action.</param>
    /// <returns>The service binding instance.</returns>
    //IServiceBinding<TService> Configure<TOptions>(Action<TOptions> configure) where TOptions : class;
}

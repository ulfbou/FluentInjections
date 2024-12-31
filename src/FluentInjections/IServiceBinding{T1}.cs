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
public interface IServiceBinding<TService> : IServiceBinding where TService : notnull
{
    /// <summary>
    /// Binds the service to a specific implementation type.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> To<TImplementation>() where TImplementation : class, TService;

    /// <summary>
    /// Binds the service to a specific implementation type.
    /// </summary>
    /// <param name="implementationType">The type of the implementation.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> To(Type implementationType);

    /// <summary>
    /// Binds the service to itself.
    /// </summary>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> AsSelf();

    /// <summary>
    /// Sets the service lifetime to singleton.
    /// </summary>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> AsSingleton();

    /// <summary>
    /// Sets the service lifetime to scoped.
    /// </summary>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> AsScoped();

    /// <summary>
    /// Sets the service lifetime to transient.
    /// </summary>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> AsTransient();

    /// <summary>
    /// Uses a factory method to create the service instance.
    /// </summary>
    /// <param name="factory">The factory method.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> WithFactory(Func<IServiceProvider, TService> factory);

    /// <summary>
    /// Names the service binding.
    /// </summary>
    /// <param name="key">The name of the binding.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> WithKey(string key);

    /// <summary>
    /// Sets a custom lifetime for the service.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> WithLifetime(ServiceLifetime lifetime);

    /// <summary>
    /// Specifies parameters for the service using a dictionary.
    /// </summary>
    /// <param name="parameters">The parameters as a dictionary.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> WithParameter(string key, object value);

    /// <summary>
    /// Specifies parameters for the service.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> WithParameters(object parameters);

    /// <summary>
    /// Specifies parameters for the service using a dictionary.
    /// </summary>
    /// <param name="parameters">The parameters as a dictionary.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> WithParameters(IReadOnlyDictionary<string, object> parameters);

    /// <summary>
    /// Uses a specific instance for the service.
    /// </summary>
    /// <param name="instance">The service instance.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> WithInstance(TService instance);

    /// <summary>
    /// Configures the service after it is created.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> Configure(Action<TService> configure);

    /// <summary>
    /// Adds metadata to the service.
    /// </summary>
    /// <param name="name">The name of the metadata.</param>
    /// <param name="value">The value of the metadata.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBinding<TService> WithMetadata(string name, object value);

    /// <summary>
    /// Configures options for the service.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    /// <param name="configure">The options configuration action.</param>
    /// <returns>The service binding instance.</returns>
    //IServiceBinding<TService> Configure<TOptions>(Action<TOptions> configure) where TOptions : class;
}
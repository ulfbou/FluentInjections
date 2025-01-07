// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public interface IServiceBindingBuilder
{
    /// <summary>
    /// Binds the service to a specific implementation type.
    /// </summary>
    /// <param name="implementationType">The type of the implementation.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder To(Type implementationType);

    /// <summary>
    /// Binds the service to itself.
    /// </summary>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder AsSelf();

    /// <summary>
    /// Sets the service lifetime to singleton.
    /// </summary>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder AsSingleton();

    /// <summary>
    /// Sets the service lifetime to scoped.
    /// </summary>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder AsScoped();

    /// <summary>
    /// Sets the service lifetime to transient.
    /// </summary>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder AsTransient();

    /// <summary>
    /// Uses a factory method to create the service instance.
    /// </summary>
    /// <param name="factory">The factory method.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder WithFactory(Func<IServiceProvider, object> factory);

    /// <summary>
    /// Names the service binding.
    /// </summary>
    /// <param name="name">The name of the binding.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder WithName(string name);

    /// <summary>
    /// Sets a custom lifetime for the service.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder WithLifetime(ServiceLifetime lifetime);

    /// <summary>
    /// Specifies parameters for the service using a dictionary.
    /// </summary>
    /// <param name="parameters">The parameters as a dictionary.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder WithParameter(string key, object? value);

    /// <summary>
    /// Specifies parameters for the service.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder WithParameters(object parameters);

    /// <summary>
    /// Specifies parameters for the service using a dictionary.
    /// </summary>
    /// <param name="parameters">The parameters as a dictionary.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder WithParameters(IReadOnlyDictionary<string, object?> parameters);

    /// <summary>
    /// Uses a specific instance for the service.
    /// </summary>
    /// <param name="instance">The service instance.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder WithInstance(object instance);

    /// <summary>
    /// Adds metadata to the service.
    /// </summary>
    /// <param name="name">The name of the metadata.</param>
    /// <param name="value">The value of the metadata.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder WithMetadata(string name, object? value);

    /// <summary>
    /// Configures the service after it is created.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The service binding instance.</returns>
    IServiceBindingBuilder Configure(Action<object> configure);
}

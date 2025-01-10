// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

/// <summary>
/// Represents a service configurator that provides methods to configure services within the application.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define service configurations.
/// </remarks>
public interface IServiceConfigurator : IConfigurator<IServiceBinding>
{
    IServiceCollection Services { get; }

    /// <summary>
    /// Binds a service to the service collection.
    /// </summary>
    /// <returns>An interface for further configuring the service binding.</returns>
    IServiceBindingBuilder<TService> Bind<TService>() where TService : notnull;

    /// <summary>
    /// Binds a service to the service collection.
    /// </summary>
    /// <param name="serviceType">The type of the service to bind.</param>
    /// <returns>An interface for further configuring the service binding.</returns>
    IServiceBindingBuilder Bind(Type serviceType);
}

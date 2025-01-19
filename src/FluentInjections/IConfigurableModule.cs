// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

/// <summary>
/// Represents a module that can be configured.
public interface IConfigurableModule : IModule
{
    /// <summary>
    /// Configures the specified service configurator.
    /// </summary>
    /// <param name="configurator">The service configurator.</param>
    void Configure(IServiceConfigurator configurator);

    /// <summary>
    /// Configures the specified middleware configurator.
    /// </summary>
    /// <param name="configurator">The middleware configurator.</param>
    void Configure(IMiddlewareConfigurator configurator);

    /// <summary>
    /// Configures the specified endpoint configurator.
    /// </summary>
    /// <param name="configurator">The endpoint configurator.</param>
    void Configure(IEndpointConfigurator configurator);
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Internal.Descriptors;

/// <summary>
/// Represents a service descriptor that provides information about a service.
/// </summary>
/// <typeparam name="TService">The type of service that the descriptor represents.</typeparam>
public class ServiceDescriptor<TService>(IServiceConfigurator configurator)
    : ServiceDescriptor(typeof(TService), configurator) where TService : notnull
{ }

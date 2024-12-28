// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

namespace FluentInjections;

/// <summary>
/// Represents a module that can be registered with a service configurator.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define service registrations and configurations.
/// </remarks>
public interface IServiceModule : IConfigurableModule<IServiceConfigurator> { }

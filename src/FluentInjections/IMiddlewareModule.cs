// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

/// <summary>
/// Represents a module that can be registered with a middleware configurator.
/// </summary>
/// <remarks>
/// This interface should be implemented by classes that define middleware registrations and configurations.
/// </remarks>
public interface IMiddlewareModule : IConfigurableModule<IMiddlewareConfigurator> { }

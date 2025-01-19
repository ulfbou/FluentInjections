// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

namespace FluentInjections.Tests.Internal.Configurators;

/// <summary>
/// A marker interface that represents a configurator that provides methods to configure components within the application.
/// </summary>
/// <typeparam name="TDescriptor">The type of the descriptor.</typeparam>
/// <typeparam name="TBinding">The type of the binding.</typeparam>
public interface ITestConfigurator<TDescriptor, TBinding> : IConfigurator<TBinding>
    where TDescriptor : class
    where TBinding : class, IBinding
{
    /// <summary>
    /// Gets the descriptors.
    /// </summary>
    /// <returns>The descriptors.</returns>
    IReadOnlyList<TDescriptor> GetDescriptors();
}

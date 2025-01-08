// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

namespace FluentInjections.Tests.Internal.Configurators;

/// <summary>
/// A marker interface that represents a test service configurator that provides methods to configure components within the application.
/// </summary>
public interface ITestServiceConfigurator : IServiceConfigurator, ITestConfigurator<ServiceBindingDescriptor, IServiceBinding> { }

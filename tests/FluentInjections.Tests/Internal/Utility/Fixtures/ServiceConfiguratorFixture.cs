// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

/// <summary>
/// Represents a service configurator fixture that provides methods to configure services within the application.
/// </summary>
internal class ServiceConfiguratorFixture
    : ConfiguratorFixture<TestNetCoreServiceConfigurator, ServiceCollection, NetCoreServiceProvider>
    , IServiceConfiguratorFixture<TestNetCoreServiceConfigurator, ServiceCollection, NetCoreServiceProvider>
{
    public ServiceConfiguratorFixture() : base() { }
}

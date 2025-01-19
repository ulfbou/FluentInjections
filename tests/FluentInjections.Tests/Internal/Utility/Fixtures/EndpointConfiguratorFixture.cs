// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.ServiceProvider;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

internal class EndpointConfiguratorFixture
    : ConfiguratorFixture<TestNetCoreEndpointConfigurator, ServiceCollection, NetCoreServiceProvider>
    , IEndpointConfiguratorFixture<TestNetCoreEndpointConfigurator, ServiceCollection, NetCoreServiceProvider>
{
    public EndpointConfiguratorFixture() : base() { }
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

public class NetCoreServiceConfiguratorFixture :
    ServiceConfiguratorFixture<NetCoreServiceConfigurator, ServiceCollection>
{
    public NetCoreServiceConfiguratorFixture() : base() { }
}

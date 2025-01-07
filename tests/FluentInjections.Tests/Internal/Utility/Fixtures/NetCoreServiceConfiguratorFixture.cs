// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Utils;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

internal class NetCoreServiceConfiguratorFixture :
    ServiceConfiguratorFixture,
    IConfiguratorFixture<NetCoreServiceConfigurator, ServiceCollection, NetCoreServiceProvider>

{
    public NetCoreServiceConfiguratorFixture() : base() { }
}

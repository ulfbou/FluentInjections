// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Utils;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

internal class NetCoreServiceConfiguratorFixture :
    ServiceConfiguratorFixture<NetCoreServiceConfigurator, ServiceCollection>
{
    public NetCoreServiceConfiguratorFixture() : base() { }

    protected override NetCoreServiceConfigurator Create()
    {
        var logger = LoggerUtility.CreateLogger<NetCoreServiceConfigurator>();
        return new NetCoreServiceConfigurator(new ServiceCollection(), logger);
    }
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

public class NetCoreMiddlewareConfiguratorFixture : MiddlewareConfiguratorFixture<NetCoreMiddlewareConfigurator<IApplicationBuilder>, ServiceCollection>
{
    public IServiceProvider Provider { get; private set; }
    public IApplicationBuilder Builder { get; private set; }

    public NetCoreMiddlewareConfiguratorFixture() : base()
    {
        Provider = Services.BuildServiceProvider();
        Builder = new ApplicationBuilder(Provider);
    }

    public override void Setup()
    {
        base.Setup();
        Provider = Services.BuildServiceProvider();
        Builder = new ApplicationBuilder(Provider);
    }
}

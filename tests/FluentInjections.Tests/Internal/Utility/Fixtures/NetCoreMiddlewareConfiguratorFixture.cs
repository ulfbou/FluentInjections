// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;
#if false
internal class NetCoreMiddlewareConfiguratorFixture
    : MiddlewareConfiguratorFixture<NetCoreMiddlewareConfigurator, ServiceCollection>
{
    public ApplicationBuilder AppBuilder { get; private set; }
    public ServiceProvider Provider { get; private set; }

    public NetCoreMiddlewareConfiguratorFixture() : base()
    {
        Provider = DependencyBuilder.BuildServiceProvider();
        AppBuilder = new ApplicationBuilder(Provider);
        Configurator = Create(); // Since Configurator's call to Create returns default value.
    }

    protected override NetCoreMiddlewareConfigurator Create()
    {
        if (AppBuilder is null)
        {
            return default!;
        }

        LoggerMock ??= new();
        return new(AppBuilder, LoggerMock.Object);
    }
}
#endif

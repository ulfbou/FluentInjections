// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

internal class ServiceConfiguratorFixture
    : ConfiguratorFixture<NetCoreServiceConfigurator, ServiceCollection, NetCoreServiceProvider>
    , IServiceConfiguratorFixture<NetCoreServiceConfigurator, ServiceCollection, NetCoreServiceProvider>
{
    internal Mock<ILogger<NetCoreServiceConfigurator>> LoggerMock { get; set; }
    public override NetCoreServiceConfigurator Configurator { get; set; }
    public NetCoreServiceProvider? Provider { get; set; }

    public ServiceConfiguratorFixture() : base()
    {
        LoggerMock = new();
        Configurator = new(Services, LoggerMock.Object);
    }

    public override void Setup()
    {
        base.Setup();
        LoggerMock = new();
        Configurator = new(Services, LoggerMock.Object);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        Configurator?.Dispose();
        Configurator = default!;
        LoggerMock = default!;
    }
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Units.Configurator;
#if false
internal sealed class InternalNetCoreMiddlewareConfiguratorTests
    : MiddlewareConfiguratorTests<NetCoreMiddlewareConfigurator, ServiceCollection, NetCoreMiddlewareConfiguratorFixture>
{
    private readonly Mock<ILogger<NetCoreMiddlewareConfigurator>> _loggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;

    internal InternalNetCoreMiddlewareConfiguratorTests() : base()
    {
        Services = new ServiceCollection();
        Fixture.DependencyBuilder = Services;

        _loggerMock = new Mock<ILogger<NetCoreMiddlewareConfigurator>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

        Services.AddSingleton<ILoggerFactory>(_loggerFactoryMock.Object);
        Services.AddSingleton<ILogger<NetCoreMiddlewareConfigurator>>(_loggerMock.Object);
        Services.AddTransient<TestMiddleware>();
        Services.AddTransient<MiddlewareA>();
        Services.AddTransient<MiddlewareB>();
    }

    internal override void BuildProvider()
    {
        if (Provider is not null)
        {
            throw new InvalidOperationException("Provider already built");
        }

        Provider = Services.BuildServiceProvider();
    }
}
#endif

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using FluentInjections.Tests.Utility.Fixtures;
using Microsoft.EntityFrameworkCore;
using FluentInjections.Internal.Utils;
using FluentInjections.Tests.Internal.Middlewares;
using Moq;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Units.Configurator;
//     : ServiceConfiguratorTests<AutofacServiceConfigurator, ContainerBuilder, AutofacServiceConfiguratorFixture>

internal sealed class InternalNetCoreMiddlewareConfiguratorTests
    : MiddlewareConfiguratorTests<NetCoreMiddlewareConfigurator<ApplicationBuilder>, ServiceCollection, NetCoreMiddlewareConfiguratorFixture>
{
    private readonly Mock<ILogger<AutofacMiddlewareConfigurator>> _loggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;

    internal InternalNetCoreMiddlewareConfiguratorTests() : base()
    {
        DependencyBuilder = new ServiceCollection();
        Fixture.DependencyBuilder = DependencyBuilder;

        // .NET Core 9.0.0 register LoggerUtility
        _loggerMock = new Mock<ILogger<AutofacMiddlewareConfigurator>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

        // Register services for 
        DependencyBuilder.AddSingleton<ILoggerFactory>(_loggerFactoryMock.Object);
        DependencyBuilder.AddSingleton<ILogger<AutofacMiddlewareConfigurator>>(_loggerMock.Object);
        DependencyBuilder.AddTransient<TestMiddleware>();
        DependencyBuilder.AddTransient<MiddlewareA>();
        DependencyBuilder.AddTransient<MiddlewareB>();
    }

    internal override void BuildProvider()
    {
        if (Provider is not null)
        {
            throw new InvalidOperationException("Provider already built");
        }

        Provider = DependencyBuilder.BuildServiceProvider();
    }
}
// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Wrappers;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Services;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

/// <summary>
/// Represents a fixture that provides methods to configure middleware within the application.
/// </summary>
internal class MiddlewareConfiguratorFixture
    : ConfiguratorFixture<TestNetCoreMiddlewareConfigurator, ServiceCollection, NetCoreServiceProvider>
    , IMiddlewareConfiguratorFixture
    , IConfiguratorFixture<IServiceConfigurator>
{
    public IServiceConfigurator ServiceConfigurator { get; set; }
    public ITestMiddlewareConfigurator MiddlewareConfigurator { get; set; }
    public Mock<ILogger<TestNetCoreServiceConfigurator>> ServiceLoggerMock { get; set; }
    public ApplicationBuilder AppBuilder { get; set; }
    public Mock<ILoggerFactory> LoggerFactoryMock { get; set; }
    public Mock<ILogger<TestNetCoreMiddlewareConfigurator>> MiddlewareLoggerMock { get; set; }

    public MiddlewareConfiguratorFixture() : base()
    {
        ServiceLoggerMock = new Mock<ILogger<TestNetCoreServiceConfigurator>>();
        MiddlewareLoggerMock = new Mock<ILogger<TestNetCoreMiddlewareConfigurator>>();
        var serviceConfigurator = new TestNetCoreServiceConfigurator(Services, ServiceLoggerMock.Object);
        ServiceConfigurator = serviceConfigurator;
        LoggerMock = new Mock<ILogger<TestNetCoreMiddlewareConfigurator>>();
        LoggerFactoryMock = new Mock<ILoggerFactory>();
        LoggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(LoggerMock.Object);

        ServiceConfigurator.Bind<ILoggerFactory>()
                           .WithInstance(LoggerFactoryMock.Object)
                           .AsSingleton();
        ServiceConfigurator.Bind<ILogger<TestNetCoreMiddlewareConfigurator>>()
                           .WithInstance(LoggerMock.Object)
                           .AsSingleton();
        ServiceConfigurator.Bind<TestMiddleware>()
                           .AsSelf()
                           .AsTransient();
        ServiceConfigurator.Bind<MiddlewareA>()
                           .AsSelf()
                           .AsTransient();
        ServiceConfigurator.Bind<MiddlewareB>()
                           .AsSelf()
                           .AsTransient();

        ServiceConfigurator.Register();

        Provider = serviceConfigurator.BuildServiceProvider();
        AppBuilder = new ApplicationBuilder(Provider);
        MiddlewareConfigurator = new TestNetCoreMiddlewareConfigurator(AppBuilder, LoggerMock.Object);
    }
}

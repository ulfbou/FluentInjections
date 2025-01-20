// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Wrappers;
using FluentInjections.Tests.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Utility.Fixtures;

internal interface IMiddlewareConfiguratorFixture : IConfiguratorFixture<TestNetCoreMiddlewareConfigurator, ServiceCollection, NetCoreServiceProvider>
{
    IServiceConfigurator ServiceConfigurator { get; set; }
    ITestMiddlewareConfigurator MiddlewareConfigurator { get; set; }
    Mock<ILogger<TestNetCoreMiddlewareConfigurator>> MiddlewareLoggerMock { get; set; }
    Mock<ILoggerFactory> LoggerFactoryMock { get; }
    ApplicationBuilder AppBuilder { get; set; }
}

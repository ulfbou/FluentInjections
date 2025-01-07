// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;
#if false
public abstract class MiddlewareConfiguratorFixture : IMiddlewareConfiguratorFixture, IConfiguratorFixture<IServiceConfigurator>
{
    public IServiceConfigurator ServiceConfigurator { get; set; }
    public IMiddlewareConfigurator MiddlewareConfigurator { get; set; }
    public IServiceCollection DependencyBuilder { get; set; }

    internal Mock<ILogger<NetCoreServiceConfigurator>> ServiceLoggerMock { get; set; }
    public Mock<ILogger<IMiddlewareConfigurator>> MiddlewareLoggerMock { get; set; }

    public void Cleanup()
    {
        ServiceConfigurator?.Dispose();
        ServiceConfigurator = null!;
        DependencyBuilder = null!;
    }

    public void Setup()
    {
        DependencyBuilder = new ServiceCollection();
        ServiceConfigurator = Create();
    }

    public MiddlewareConfiguratorFixture()
    {
        ServiceLoggerMock = new Mock<ILogger<NetCoreServiceConfigurator>>();
        MiddlewareLoggerMock = new Mock<ILogger<IMiddlewareConfigurator>>();
        DependencyBuilder = new ServiceCollection();
        ServiceConfigurator = new NetCoreServiceConfigurator(DependencyBuilder, ServiceLoggerMock.Object);
        RegisterDependencies();
        MiddlewareConfigurator = Create();
    }

    private void RegisterDependencies() => throw new NotImplementedException();

    private IMiddlewareConfigurator Create()
    {
        if (DependencyBuilder is null)
        {
            return default!;
        }
        return new NetCoreMiddlewareConfigurator(DependencyBuilder, MiddlewareLoggerMock.Object);
    }
}
#endif

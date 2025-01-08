// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Units.Configurator;

/// <summary>
/// Represents a class for testing <see cref="NetCoreMiddlewareConfigurator"/> and <see cref="IMiddlewareConfigurator"/>.
/// </summary>
internal sealed class InternalNetCoreMiddlewareConfiguratorTests
    : MiddlewareConfiguratorTests<TestNetCoreMiddlewareConfigurator, ServiceCollection, NetCoreServiceProvider, MiddlewareConfiguratorFixture>
{
    public Mock<ILogger<TestNetCoreMiddlewareConfigurator>> LoggerMock { get; set; }
    public Mock<ILoggerFactory> LoggerFactoryMock { get; set; }
    public ApplicationBuilder AppBuilder { get; set; }
    internal override TestNetCoreMiddlewareConfigurator Configurator { get; set; }
    internal TestNetCoreServiceConfigurator ServiceConfigurator { get; set; }
    internal override NetCoreServiceProvider? Provider { get; set; }


    internal InternalNetCoreMiddlewareConfiguratorTests() : base()
    {
        LoggerMock = Fixture.LoggerMock;
        LoggerFactoryMock = Fixture.LoggerFactoryMock;
        AppBuilder = Fixture.AppBuilder;
        Configurator = Fixture.MiddlewareConfigurator as TestNetCoreMiddlewareConfigurator ?? throw new InvalidOperationException("Invalid middleware configurator");
        ServiceConfigurator = Fixture.ServiceConfigurator as TestNetCoreServiceConfigurator ?? throw new InvalidOperationException("Invalid service configurator");
    }

    public async Task Register_ShouldInvokeRegisterMethodForEachDescriptorAsync()
    {
        // Arrange
        var descriptorA = new MiddlewareBindingDescriptor(typeof(MiddlewareA), Configurator);
        var descriptorB = new MiddlewareBindingDescriptor(typeof(MiddlewareB), Configurator);
        var mockRegisterAction = new Mock<Action<MiddlewareBindingDescriptor, HttpContext>>();
        Configurator = new TestNetCoreMiddlewareConfigurator(Fixture.AppBuilder, Fixture.LoggerMock.Object, new[] { descriptorA, descriptorB });

        // Act
        Configurator.RegisterWithAction(mockRegisterAction.Object);
        var context = new DefaultHttpContext();
        var middleware = AppBuilder.Build();
        await middleware(context);

        // Assert
        mockRegisterAction.Verify(action => action(descriptorA, It.IsAny<HttpContext>()), Times.Once);
        mockRegisterAction.Verify(action => action(descriptorB, It.IsAny<HttpContext>()), Times.Once);
        mockRegisterAction.Verify(action => action(It.IsAny<MiddlewareBindingDescriptor>(), It.IsAny<HttpContext>()), Times.Exactly(2));
    }

    ///<inheritdoc />
    internal override void BuildProvider()
    {
        if (Provider is not null)
        {
            throw new InvalidOperationException("Provider already built");
        }

        Provider = ServiceConfigurator.BuildServiceProvider();
        AppBuilder = new ApplicationBuilder(Provider);
    }
}

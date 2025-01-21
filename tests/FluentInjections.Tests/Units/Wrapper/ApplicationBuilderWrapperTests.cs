// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Wrappers;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Modules;
using FluentInjections.Tests.Internal.Wrappers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FluentInjections.Tests.Units.Wrapper;

public class ApplicationBuilderWrapperTests
{
    public WebApplicationBuilder AppBuilder { get; }
    public WebApplication InnerApp { get; }
    public IApplicationBuilder InnerBuilder => InnerApp;
    public IServiceProvider Provider { get; set; }
    internal NetCoreMiddlewareConfigurator Configurator { get; }
    public ILogger<ApplicationBuilderWrapper> Logger { get; }
    internal TestApplicationBuilderWrapper Wrapper { get; }
    public Mock<Func<RequestDelegate, RequestDelegate>> Middleware { get; }

    public ApplicationBuilderWrapperTests()
    {
        AppBuilder = WebApplication.CreateBuilder();

        AppBuilder.Services.AddSingleton<ILogger<NetCoreMiddlewareConfigurator>>(_ => Mock.Of<ILogger<NetCoreMiddlewareConfigurator>>());

        AppBuilder.Services.AddSingleton<IMiddlewareConfigurator, TestNetCoreMiddlewareConfigurator>();

        AppBuilder.Services.AddSingleton<IApplicationBuilder>(_ => InnerApp!);
        AppBuilder.Services.AddSingleton<IServiceProvider>(_ => InnerApp!.Services);
        InnerApp = AppBuilder.Build();
        Provider = InnerApp.Services;

        Configurator = new NetCoreMiddlewareConfigurator(InnerBuilder, Provider, Mock.Of<ILogger<NetCoreMiddlewareConfigurator>>());
        Logger = Mock.Of<ILogger<ApplicationBuilderWrapper>>();
        Wrapper = new TestApplicationBuilderWrapper(InnerBuilder, Configurator, Logger);
        Middleware = new Mock<Func<RequestDelegate, RequestDelegate>>();
    }

    [Fact]
    public void Use_AddsMiddlewareToDescriptors()
    {
        // Arrange
        // Act
        Wrapper.Use(Middleware.Object);

        // Assert
        Wrapper.GetMiddlewareDescriptors().Should().HaveCount(1);
        Wrapper.GetMiddlewareDescriptors().First().Instance.Should().Be(Middleware.Object);
    }

    [Fact]
    public void UseMiddleware_AddsDescriptorWithCorrectType()
    {
        // Arrange
        var expectedType = typeof(TestMiddleware);

        // Act
        Wrapper.Use(typeof(TestMiddleware));

        // Assert
        var descriptor = Wrapper.GetMiddlewareDescriptors().Single();
        descriptor.GetType().Should().Be(typeof(MiddlewareDescriptor));
        descriptor.MiddlewareType.Should().Be(expectedType);
    }

    [Fact]
    public void Use_CallsConfigurationAction()
    {
        // Arrange
        var mockAction = new Mock<Action<IApplicationBuilder>>();

        // Act
        Wrapper.Use(mockAction.Object);

        // Assert
        mockAction.Verify(a => a(Wrapper), Times.Once);
    }

    [Fact]
    public async Task BuildAsync_SortsAndExecutesModulesConcurrently()
    {
        // Arrange
        var module1 = new Mock<IMiddlewareModule>();
        var module2 = new Mock<IMiddlewareModule>();

        Wrapper.RegisterModule(module1.Object);
        Wrapper.RegisterModule(module2.Object);

        var module1Invoked = false;
        var module2Invoked = false;

        module1.Setup(m => m.Configure(It.IsAny<IMiddlewareConfigurator>()))
            .Callback(() => module1Invoked = true);
        module2.Setup(m => m.Configure(It.IsAny<IMiddlewareConfigurator>()))
            .Callback(() => module2Invoked = true);

        // Act
        await Wrapper.BuildAsync();

        // Assert
        Assert.True(module1Invoked);
        Assert.True(module2Invoked);
    }
}

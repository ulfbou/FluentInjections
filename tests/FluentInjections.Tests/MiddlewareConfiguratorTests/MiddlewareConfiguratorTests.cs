using FluentInjections.Tests.Middleware;
using FluentInjections.Tests.Utilities;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using Moq;

using Serilog;

using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Xml.Linq;

using Xunit;

namespace FluentInjections.Tests.MiddlewareConfiguratorTests;

public class MiddlewareConfiguratorTests : IClassFixture<TestFixture>
{
    private readonly ILogger<MiddlewareConfiguratorTests> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MiddlewareConfiguratorTests(TestFixture fixture)
    {
        _logger = fixture.ServiceProvider.GetRequiredService<ILogger<MiddlewareConfiguratorTests>>();
        _serviceProvider = fixture.ServiceProvider;
    }

    [Fact]
    public void Builder_ReturnsCorrectBuilderInstance()
    {
        var builder = new ApplicationBuilder(_serviceProvider);
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builder, _serviceProvider);

        Assert.Equal(builder, configurator.Builder);
    }

    [Fact]
    public void Builder_WithMultipleMiddleware_ReturnsCorrectBuilderInstance()
    {
        var builder = new ApplicationBuilder(_serviceProvider);
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builder, _serviceProvider);
        configurator.Use<TestMiddleware>();
        configurator.Use<TestMiddleware>();
        Assert.Equal(builder, configurator.Builder);
    }

    [Fact]
    public void UseMiddleware_AddsMiddlewareToPipeline()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger<MiddlewarePipelineBuilder>>();
        var builder = new MiddlewarePipelineBuilder(services, loggerMock.Object);

        // Act
        builder.UseMiddleware<TestMiddleware>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var middleware = serviceProvider.GetService<TestMiddleware>();
        Assert.NotNull(middleware);
    }

    // Use_ValidMiddlewareType_AddsMiddleware
    // This test checks whether a valid middleware type gets added to the pipeline correctly.
    //
    // Setup: Mock the IApplicationBuilder or similar pipeline structure.Use a mock middleware type.
    // Action: Call the method under test with the valid middleware type.
    // Assertion: Assert that the middleware type was added using a spy/mock.
    // Best Practices:
    // Use mocking frameworks like Moq to verify the pipeline behavior
    // Ensure the middleware type is validated correctly (e.g., derived from a base type).
    // Use a spy to verify the middleware was added to the pipeline.
    [Fact]
    public void Use_ValidMiddlewareType_AddsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger<MiddlewarePipelineBuilder>>();
        var builder = new MiddlewarePipelineBuilder(services, loggerMock.Object);

        // Act
        builder.UseMiddleware<TestMiddleware>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var middleware = serviceProvider.GetService<TestMiddleware>();
        Assert.NotNull(middleware);
    }

    // Use_InvalidMiddlewareType_ThrowsException
    // This test checks whether an invalid middleware type throws an exception.
    //
    // Setup: 
    // Action: 
    // Assertion: Use Assert.Throws to verify the exception type and message.
    // Best Practices:
    // Test various invalid cases (e.g., null, unrelated types).
    // Use parameterized tests ([Theory]) to cover multiple invalid scenarios efficiently.
    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(object))]
    public void Use_InvalidMiddlewareType_ThrowsException(Type type)
    {
        // Arrange
        // Prepare an invalid middleware type (e.g., a type not derived from IMiddleware or equivalent).
        var services = new ServiceCollection();
        var loggerMock = new Mock<ILogger<MiddlewarePipelineBuilder>>();
        var builder = new MiddlewarePipelineBuilder(services, loggerMock.Object);
        var configurator = new MiddlewareConfigurator<IMiddlewarePipelineBuilder>(builder, services.BuildServiceProvider());

        // Act
        // Invoke the method under test with the invalid middleware type.
        void Action() => configurator.Use(type);

        // Assert
        // Verify that the method throws an exception.
        Assert.Throws<ArgumentException>(Action);
    }

    // Builder_WithMultipleNamedMiddleware_ExecutesInOrderAsync
    // This test ensures multiple named middleware instances are executed in the expected order.
    // 
    // Setup: Use an in-memory test harness or mock pipeline to verify the order of execution.
    // Action: Add multiple named middleware instances to the builder and execute the pipeline.
    // Assertion: Validate that middleware were invoked in the correct sequence using mocks or state tracking.
    // Best Practices:
    // 
    // Use an IMiddleware implementation with a side effect(e.g., recording the order of execution).
    // Consider ILogger mocks for verifying expected execution flow.
    // Ensure asynchronous operations are awaited properly using Task utilities.



    private class TestMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return next(context);
        }
    }
}

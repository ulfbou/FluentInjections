using FluentInjections.Tests.Middleware;
using FluentInjections.Tests.Utilities;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using Serilog;

using System.Reflection;

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

    private class TestMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return next(context);
        }
    }
}

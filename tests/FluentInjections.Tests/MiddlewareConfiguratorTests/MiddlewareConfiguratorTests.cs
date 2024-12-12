using FluentInjections.Tests.Middleware;
using FluentInjections.Tests.Utilities;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

    private class TestMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return next(context);
        }
    }

    [Fact]
    public void Use_ValidMiddlewareType_AddsMiddleware()
    {
        var builder = new ApplicationBuilder(_serviceProvider);
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builder, _serviceProvider);

        configurator.Use<TestMiddleware>();

        var middlewareField = builder.GetType().GetField("_components", BindingFlags.NonPublic | BindingFlags.Instance);
        var components = middlewareField?.GetValue(builder) as IList<Func<RequestDelegate, RequestDelegate>>;

        Assert.NotNull(components);
        Assert.Contains(components, component => component?.Target?.GetType() == typeof(TestMiddleware));
    }

    [Fact]
    public void Use_InvalidMiddlewareType_ThrowsArgumentException()
    {
        var builder = new ApplicationBuilder(_serviceProvider);
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builder, _serviceProvider);

        Assert.Throws<ArgumentException>(() => configurator.Use(typeof(string)));
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
    public async Task Builder_WithMultipleNamedMiddleware_ExecutesInOrderAsync()
    {
        IMiddlewarePipelineBuilder builder = new MiddlewarePipelineBuilder(new ServiceCollection());
        var iterationList = new List<string>();

        builder.UseMiddleware<NamedMiddleware>("A", iterationList);
        builder.UseMiddleware<NamedMiddleware>("B", iterationList);
        builder.UseMiddleware<NamedMiddleware>("C", iterationList);
        RequestDelegate pipeline = builder.Build();

        _logger.LogInformation("Executing middleware pipeline.");
        _logger.LogInformation("Iteration list before executing pipeline: {IterationList}", string.Join(", ", iterationList));
        var context = new DefaultHttpContext();
        await pipeline(context);
        _logger.LogInformation("Iteration list after executing pipeline: {IterationList}", string.Join(", ", iterationList));

        Assert.Equal(6, iterationList.Count);
        Assert.Equal("A", iterationList[0]);
        Assert.Equal("A", iterationList[1]);
        Assert.Equal("B", iterationList[2]);
        Assert.Equal("B", iterationList[3]);
        Assert.Equal("C", iterationList[4]);
        Assert.Equal("C", iterationList[5]);
    }
}

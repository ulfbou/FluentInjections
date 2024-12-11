using FluentInjections.Tests.Utilities;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

namespace FluentInjections.Tests;

public class MiddlewareConfiguratorTests
{
    // Constructor Tests
    [Fact]
    public void Constructor_InitializesBuilderCorrectly()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        Assert.Equal(builderMock.Object, configurator.Builder);
    }

    [Fact]
    public void Constructor_ThrowsOnNullBuilder()
    {
        Assert.Throws<ArgumentNullException>(() => new MiddlewareConfigurator<IApplicationBuilder>(null!));
    }

    // Type Validation Tests
    [Fact]
    public void Use_ThrowsException_IfMiddlewareTypeDoesNotImplementIMiddleware()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        Assert.Throws<ArgumentException>(() => configurator.Use(typeof(InvalidMiddleware)));
    }

    [Fact]
    public void Use_RegistersMiddleware_IfTypeImplementsIMiddleware()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        configurator.Use(typeof(ValidMiddleware));

        builderMock.Verify(b => b.UseMiddleware<ValidMiddleware>(It.IsAny<object[]>()), Times.Once);
    }

    // Supported Builder Tests
    [Fact]
    public void Use_RegistersMiddleware_ForIApplicationBuilder()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        configurator.Use<ValidMiddleware>();

        builderMock.Verify(b => b.UseMiddleware<ValidMiddleware>(It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void Use_RegistersMiddleware_ForWebApplication()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        var configurator = new MiddlewareConfigurator<WebApplication>(app);

        configurator.Use<ValidMiddleware>();

        Assert.Contains(builder.Services, s => s.ServiceType == typeof(ValidMiddleware));
    }

    [Fact]
    public void Use_ThrowsException_ForUnsupportedBuilder()
    {
        var builderMock = new Mock<object>();
        var configurator = new MiddlewareConfigurator<object>(builderMock.Object);

        Assert.Throws<NotSupportedException>(() => configurator.Use<ValidMiddleware>());
    }

    // Argument Handling Tests
    [Fact]
    public void Use_PassesMiddlewareArgumentsCorrectly()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        configurator.Use<ConstructorMiddleware>(1, "test");

        builderMock.Verify(b => b.UseMiddleware<ConstructorMiddleware>(It.Is<object[]>(args => args.Length == 2)), Times.Once);
    }

    [Fact]
    public void Use_RegistersMiddleware_WithNoArguments()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        configurator.Use<ValidMiddleware>();

        builderMock.Verify(b => b.UseMiddleware<ValidMiddleware>(It.Is<object[]>(args => args.Length == 0)), Times.Once);
    }

    // Generic Method Tests
    [Fact]
    public void Use_Generic_CallsTypeMethod()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        configurator.Use<ValidMiddleware>();

        builderMock.Verify(b => b.UseMiddleware<ValidMiddleware>(It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void Use_Generic_ThrowsException_ForInvalidMiddlewareType()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        Assert.Throws<ArgumentException>(() => configurator.Use(typeof(InvalidMiddleware)));
    }

    // Test to validate middleware registration
    [Fact]
    public async Task Middleware_IsAddedToApplicationPipeline()
    {
        // Arrange
        var builder = HostBuilderUtility.CreateHostBuilder();

        IHost? host = null;
        try
        {
            host = await builder.StartAsync();

            // Act
            var client = host.GetTestClient();
            var response = await client.GetAsync("/");
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("Hello, world!", responseString);
        }
        finally
        {
            await host!.StopAsync();
        }
    }

    [Fact]
    public void Middleware_PipelineOrderIsMaintained()
    {
        // Arrange
        var order = new List<string>();
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        // Middleware invocations will add to the order list
        builderMock.Setup(app => app.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>())).Callback<Func<RequestDelegate, RequestDelegate>>(middleware =>
        {
            var next = new RequestDelegate(context => Task.CompletedTask);
            var wrapped = middleware(next);
            wrapped.Invoke(new DefaultHttpContext()).Wait();
        });

        // Register middlewares
        configurator.Use<FirstMiddleware>();
        configurator.Use<SecondMiddleware>();

        // Act - Simulate pipeline execution by invoking the middlewares
        var requestDelegate = new RequestDelegate(context => Task.CompletedTask);
        builderMock.Object.Use(next => { order.Add("Middleware1"); return next; });
        builderMock.Object.Use(next => { order.Add("Middleware2"); return next; });

        // Assert
        Assert.Equal(new List<string> { "FirstMiddleware", "SecondMiddleware", "Middleware1", "Middleware2" }, order);
    }

    // Edge Cases
    [Fact]
    public void Use_ThrowsException_ForNullMiddlewareType()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        Assert.Throws<ArgumentNullException>(() => configurator.Use(null));
    }

    [Fact]
    public void Use_RegistersMiddleware_WithEmptyArguments()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        configurator.Use<ValidMiddleware>(Array.Empty<object?>());

        builderMock.Verify(b => b.UseMiddleware<ValidMiddleware>(It.Is<object[]>(args => args.Length == 0)), Times.Once);
    }

    [Fact]
    public void Use_RegistersSameMiddlewareTypeMultipleTimes()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        configurator.Use<ValidMiddleware>();
        configurator.Use<ValidMiddleware>();

        builderMock.Verify(b => b.UseMiddleware<ValidMiddleware>(It.IsAny<object[]>()), Times.Exactly(2));
    }

    // Exception Tests
    [Fact]
    public void Use_ThrowsDetailedException_OnInvalidMiddlewareLifecycle()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(builderMock.Object);

        var exception = Assert.Throws<ArgumentException>(() => configurator.Use(typeof(InvalidMiddleware)));
        Assert.Contains("middlewareType", exception.Message);
        Assert.Contains("Builder", exception.Message);
    }

    // Valid Middleware Class for Thread Safety and Generic Tests
    public class ValidMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return next(context);
        }
    }

    // Invalid Middleware Class
    public class InvalidMiddleware { }

    // Middleware with Constructor Arguments
    public class ConstructorMiddleware : IMiddleware
    {
        public ConstructorMiddleware(int value, string text) { }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return next(context);
        }
    }

    // Another Middleware for Order Testing
    public class FirstMiddleware : IMiddleware
    {
        private readonly List<string> _order;
        public FirstMiddleware(List<string> order)
        {
            _order = order;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _order.Add("FirstMiddleware");
            await next(context);
        }
    }

    public class SecondMiddleware : IMiddleware
    {
        private readonly List<string> _order; public SecondMiddleware(List<string> order) { _order = order; }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _order.Add("SecondMiddleware");
            await next(context);
        }
    }
}

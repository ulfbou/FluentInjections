using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

using Moq;

using System.Diagnostics;

namespace FluentInjections.Tests.ConfiguratorTests;

public class MiddlewareConfiguratorTests
{
    private IApplicationBuilder _application;
    private IServiceCollection _services;
    private readonly IWebHostBuilder _builder;
    private MiddlewareConfigurator<IApplicationBuilder> _configurator;
    private readonly List<Type> _pipelineOrder = new();
    private readonly Mock<IApplicationBuilder> _mockApplicationBuilder;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly List<Type> _pipelineOrder;

    public MiddlewareConfiguratorTests()
    {
        _mockApplicationBuilder = new Mock<IApplicationBuilder>();
        _services = new ServiceCollection();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _pipelineOrder = new List<Type>();

        // Setup mock service provider
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IApplicationBuilder)))
                .Returns(_mockApplicationBuilder.Object);
            _services.AddSingleton(_mockServiceProvider.Object);

        _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, _mockApplicationBuilder.Object);
         _builder = new WebHostBuilder();
        _builder.UseEnvironment("Development")
                .ConfigureServices(services =>
                {
                    services.AddTransient<MiddlewareA>(sp => new MiddlewareA(_pipelineOrder));
                    services.AddTransient<MiddlewareB>(sp => new MiddlewareB(_pipelineOrder));
                    services.AddTransient<MiddlewareC>(sp => new MiddlewareC(_pipelineOrder));
                    services.AddTransient<MiddlewareD>(sp => new MiddlewareD(_pipelineOrder));
                    services.AddTransient<MiddlewareE>(sp => new MiddlewareE(_pipelineOrder));
                    services.AddTransient<MiddlewareF>(sp => new MiddlewareF(_pipelineOrder));
                }); 
    }

    [Fact]
    public async Task Middleware_Should_Respect_PriorityAsync()
    {
        // Arrange
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddTransient<MiddlewareA>(sp => new MiddlewareA(_pipelineOrder));
                services.AddTransient<MiddlewareB>(sp => new MiddlewareB(_pipelineOrder));
            })
            .Configure(app =>
            {
                _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, app);
                    _configurator.UseMiddleware<MiddlewareA>().WithPriority(2);
                    _configurator.UseMiddleware<MiddlewareB>().WithPriority(1);
                _configurator.Register();

                app.Run(async context =>
                {
                    await context.Response.WriteAsync("Endpoint");
                });
            });

            using var server = new TestServer(builder);
            using var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(2, _pipelineOrder.Count);
            Assert.Equal(typeof(MiddlewareB), _pipelineOrder[0]);
            Assert.Equal(typeof(MiddlewareA), _pipelineOrder[1]);
        }

        // Mock Middleware classes
        private class TestMiddlewareBase : IMiddleware
        {
            private readonly List<Type> _pipelineOrder;

            public TestMiddlewareBase(List<Type> pipelineOrder)
            {
                _pipelineOrder = pipelineOrder;
            }

            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
            {
                Debug.WriteLine($"Invoking {this.GetType().Name}");
                _pipelineOrder.Add(this.GetType());
                await next(context);
            }
        }

        private class MiddlewareA : TestMiddlewareBase
        {
            public MiddlewareA(List<Type> pipelineOrder) : base(pipelineOrder) { }
        }

        private class MiddlewareB : TestMiddlewareBase
        {
            public MiddlewareB(List<Type> pipelineOrder) : base(pipelineOrder) { }
        }
    }

    [Fact]
    public async Task Middleware_Should_Respect_PriorityAsync()
    {
        // Arrange
        _builder.Configure(app =>
        {
            _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, app);
            _configurator.UseMiddleware<MiddlewareA>().WithPriority(2);
            _configurator.UseMiddleware<MiddlewareB>().WithPriority(1);
            _configurator.Register();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Endpoint");
            });
        });
        using var server = new TestServer(_builder);
        using var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();

        Assert.Equal(2, _pipelineOrder.Count);
        Assert.Equal(typeof(MiddlewareB), _pipelineOrder[0]);
        Assert.Equal(typeof(MiddlewareA), _pipelineOrder[1]);
    }

    [Fact]
    public async Task Middleware_With_Condition_Should_Be_ConditionalAsync()
    {
        _builder.Configure(app =>
        {
            _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, app);
            _configurator.UseMiddleware<MiddlewareA>().When(() => false);
            _configurator.UseMiddleware<MiddlewareB>().When(() => true);
            _configurator.Register();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Endpoint");
            });
        });

        using var server = new TestServer(_builder);
        using var client = server.CreateClient();

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        Assert.Single(_pipelineOrder);
        Assert.Equal(typeof(MiddlewareB), _pipelineOrder[0]);
    }

    [Fact]
    public async Task Middleware_With_Group_Should_Respect_GroupingAsync()
    {
        _builder.Configure(app =>
            {
                _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, app);
                _configurator.UseMiddleware<MiddlewareA>().InGroup("Group1");
                _configurator.UseMiddleware<MiddlewareB>().InGroup("Group2");
                _configurator.UseMiddleware<MiddlewareC>().InGroup("Group1");
                _configurator.Register();

                app.Run(async context =>
                {
                    await context.Response.WriteAsync("Endpoint");
                });
            });

        using var server = new TestServer(_builder);
        using var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(3, _pipelineOrder.Count);
        Assert.Equal(typeof(MiddlewareA), _pipelineOrder[0]);
        Assert.Equal(typeof(MiddlewareC), _pipelineOrder[1]);
        Assert.Equal(typeof(MiddlewareB), _pipelineOrder[2]);
    }

    [Fact]
    public async Task Middleware_With_Dependencies_Should_Be_Ordered_CorrectlyAsync()
    {
        // Arrange
        _builder.Configure(app =>
        {
            _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, app);
            _configurator.UseMiddleware<MiddlewareA>().DependsOn<MiddlewareC>();
            _configurator.UseMiddleware<MiddlewareB>().DependsOn<MiddlewareA>();
            _configurator.UseMiddleware<MiddlewareC>();
            _configurator.Register();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Endpoint");
            });
        });

        using var server = new TestServer(_builder);
        using var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(3, _pipelineOrder.Count);
        Assert.Equal(typeof(MiddlewareC), _pipelineOrder[0]);
        Assert.Equal(typeof(MiddlewareA), _pipelineOrder[1]);
        Assert.Equal(typeof(MiddlewareB), _pipelineOrder[2]);
    }

    [Fact]
    public async Task Middleware_With_Complex_Dependencies_Should_Be_Ordered_CorrectlyAsync()
    {
        // Arrange
        var registered = new List<Type>();
        _builder.Configure(app =>
        {
            _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, app);

            // Complex dependency graph:
            // D depends on B and C
            // C depends on A
            // B depends on A
            // E depends on D
            // F has no dependencies

            _configurator.UseMiddleware<MiddlewareD>().DependsOn<MiddlewareB>().DependsOn<MiddlewareC>();
            _configurator.UseMiddleware<MiddlewareC>().DependsOn<MiddlewareA>();
            _configurator.UseMiddleware<MiddlewareB>().DependsOn<MiddlewareA>();
            _configurator.UseMiddleware<MiddlewareA>();
            _configurator.UseMiddleware<MiddlewareE>().DependsOn<MiddlewareD>();
            _configurator.UseMiddleware<MiddlewareF>();
            _configurator.Register((descriptor, context, app) =>
            {
                registered.Add(descriptor.MiddlewareType);
            });

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Endpoint");
            });
        });

        using var server = new TestServer(_builder);
        using var client = server.CreateClient();

        // Act & Assert
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();

        Assert.Equal(6, registered.Count);
        Assert.Equal(typeof(MiddlewareA), registered[0]);
        Assert.Equal(typeof(MiddlewareC), registered[1]);
        Assert.Equal(typeof(MiddlewareB), registered[2]);
        Assert.Equal(typeof(MiddlewareD), registered[3]);
        Assert.Equal(typeof(MiddlewareE), registered[4]);
        Assert.Equal(typeof(MiddlewareF), registered[5]);
    }

    [Fact]
    public async Task Middleware_With_Circular_Dependency_Should_Throw_ExceptionAsync()
    {
        _builder.Configure(app =>
        {
            _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, app);

            // Circular dependency: A depends on B, B depends on C, C depends on A
            _configurator.UseMiddleware<MiddlewareA>().DependsOn<MiddlewareB>();
            _configurator.UseMiddleware<MiddlewareB>().DependsOn<MiddlewareC>();
            _configurator.UseMiddleware<MiddlewareC>().DependsOn<MiddlewareA>();
            _configurator.Register();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Endpoint");
            });
        });


        using var server = new TestServer(_builder);
        using var client = server.CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetAsync("/"));
    }

    [Fact]
    public async Task Middleware_With_Precedence_and_Following_Should_Be_Ordered_CorrectlyAsync()
    {
        _builder.Configure(app =>
        {
            _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, app);

            _configurator.UseMiddleware<MiddlewareA>().Precedes<MiddlewareC>();
            _configurator.UseMiddleware<MiddlewareB>().Follows<MiddlewareA>();
            _configurator.UseMiddleware<MiddlewareC>();
            _configurator.Register();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Endpoint");
            });
        });

        using var server = new TestServer(_builder);
        using var client = server.CreateClient();

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        Assert.Equal(3, _pipelineOrder.Count);
        Assert.Equal(typeof(MiddlewareA), _pipelineOrder[0]);
        Assert.Equal(typeof(MiddlewareB), _pipelineOrder[1]);
        Assert.Equal(typeof(MiddlewareC), _pipelineOrder[2]);
    }

    [Fact]
    public async Task Middleware_With_Precedence_and_Following_Circular_Dependency_Should_Throw_ExceptionAsync()
    {
        _builder.Configure(app =>
        {
            _configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, app);

            _configurator.UseMiddleware<MiddlewareA>().Precedes<MiddlewareB>();
            _configurator.UseMiddleware<MiddlewareB>().Precedes<MiddlewareC>();
            _configurator.UseMiddleware<MiddlewareC>().Precedes<MiddlewareA>();
            _configurator.Register();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Endpoint");
            });
        });

        using var server = new TestServer(_builder);
        using var client = server.CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetAsync("/"));
    }

    // Middleware stubs
    private class TestMiddlewareBase : IMiddleware
    {
        private readonly List<Type> _pipelineOrder;

        public TestMiddlewareBase(List<Type> pipelineOrder)
        {
            _pipelineOrder = pipelineOrder;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Debug.WriteLine($"Invoking {this.GetType().Name}");
            _pipelineOrder.Add(this.GetType());
            await next(context);
        }
    }

    private class MiddlewareA(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
    private class MiddlewareB(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
    private class MiddlewareC(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
    private class MiddlewareD(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
    private class MiddlewareE(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
    private class MiddlewareF(List<Type> pipelineOrder) : TestMiddlewareBase(pipelineOrder) { }
}

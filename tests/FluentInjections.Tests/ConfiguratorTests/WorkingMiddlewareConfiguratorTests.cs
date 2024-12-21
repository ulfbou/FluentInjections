using Autofac.Extensions.DependencyInjection;
using Autofac;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Registries;
using FluentInjections.Tests.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using FluentInjections.Tests.Internal.Extensions;

namespace FluentInjections.Tests.ConfiguratorTests;

public class WorkingMiddlewareConfiguratorTests
{
    private readonly IServiceCollection _services;
    private AutofacServiceProvider _provider;
    private ApplicationBuilder _appBuilder;

    public WorkingMiddlewareConfiguratorTests()
    {
        _services = new ServiceCollection();

        // Register FluentInjections and necessary services
        _services.AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp));
        _services.AddTransient<TestMiddleware>(sp => new TestMiddleware());
        _services.Configure<TestOptions>(options => options.Value = "Test");
        _services.AddFluentInjections<IApplicationBuilder, ModuleRegistry<IApplicationBuilder>>(typeof(TestMiddleware).Assembly);

        // Create an Autofac container builder
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(_services);

        // Build the intermediate service provider
        var intermediateContainer = containerBuilder.Build();

        // Register ILifetimeScope
        _services.AddSingleton(intermediateContainer.Resolve<ILifetimeScope>());
        _provider = new AutofacServiceProvider(intermediateContainer);
        _appBuilder = new ApplicationBuilder(_provider);
    }

    [Fact]
    public async Task Middleware_Should_Be_RegisteredAsync()
    {
        // Arrange
        var serviceProvider = _provider;
        var appBuilder = _appBuilder;

        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, appBuilder);
        var middlewareExecuted = false;

        configurator.UseMiddleware<TestMiddleware>();
        configurator.Register((descriptor, context, builder) =>
        {
            middlewareExecuted = true;
        });

        // Act
        var pipeline = BuildPipeline();
        await pipeline.Invoke(new DefaultHttpContext());

        // Assert
        Assert.True(middlewareExecuted);
        Assert.True(TestMiddleware.CallOrder.Count == 1);
    }

    private RequestDelegate BuildPipeline()
    {
        return _appBuilder.Build();
    }

    internal void Callback(IMiddlewareBinding binding, Action<MiddlewareDescriptor> action) => binding.Callback(action);
}
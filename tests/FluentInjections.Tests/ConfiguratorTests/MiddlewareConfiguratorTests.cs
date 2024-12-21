using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Registries;
using FluentInjections.Tests.Internal;
using FluentInjections.Tests.Internal.Extensions;
using FluentInjections.Tests.Internal.Helpers;
using FluentInjections.Tests.Middlewares;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using System.Diagnostics;

using Xunit;

using static FluentInjections.Tests.ConfiguratorTests.ServiceConfiguratorTests;

namespace FluentInjections.Tests.ConfiguratorTests;

public class MiddlewareConfiguratorTests
{
    private readonly IServiceCollection _services;
    private AutofacServiceProvider _provider;
    private ApplicationBuilder _appBuilder;

    public MiddlewareConfiguratorTests()
    {
        _services = new ServiceCollection();

        // Register FluentInjections and necessary services
        _services.AddLogging();
        _services.AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp));
        _services.AddTransient<TestMiddleware>(sp => new TestMiddleware());
        _services.Configure<TestOptions>(options => options.Value = "Test");
        _services.AddFluentInjections<IApplicationBuilder>(typeof(TestMiddleware).Assembly);

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
    public async Task Middleware_Should_Be_Registered_With_OptionsAsync()
    {
        // Arrange
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, _appBuilder);
        var middlewareExecuted = false;
        var testOptions = new TestOptions { Value = "Non-Optional" };

        configurator.UseMiddleware<TestMiddleware<TestOptions>>()
                    .WithOptions(testOptions);
        configurator.Register((descriptor, context, builder) =>
        {
            middlewareExecuted = true;
        });

        // Act
        var pipeline = BuildPipeline(_appBuilder);
        var context = new DefaultHttpContext();
        await pipeline.Invoke(context);

        var sp = _appBuilder.ApplicationServices;
        var actualOptions = sp.GetRequiredService<TestOptions>();

        // Assert
        Assert.True(middlewareExecuted);
        Assert.Equal(1, TestMiddleware<TestOptions>.CallOrder?.Count);
        Assert.Equal(testOptions.Value, TestMiddleware<TestOptions>.LastOptions.Value);
    }

    private RequestDelegate BuildPipeline(IApplicationBuilder appBuilder)
    {
        return appBuilder.Build();
    }

    internal void Callback(IMiddlewareBinding binding, Action<MiddlewareDescriptor> action) => binding.Callback(action);
}

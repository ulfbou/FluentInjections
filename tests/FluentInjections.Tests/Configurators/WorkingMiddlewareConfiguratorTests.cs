using FluentInjections;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Registries;
using FluentInjections.Tests.Internal.Extensions;
using FluentInjections.Tests.Middlewares;
using FluentInjections.Tests.Modules;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FluentInjections.Tests.Configurators;

public class WorkingMiddlewareConfiguratorTests
{
    private readonly IServiceCollection _services;
    private IServiceProvider _provider;
    private IApplicationBuilder _appBuilder;

    public WorkingMiddlewareConfiguratorTests()
    {
        _services = new ServiceCollection();

        // Register FluentInjections and necessary services
        _services.AddLogging();
        _services.AddTransient<IApplicationBuilder>(sp => new ApplicationBuilder(sp));
        _services.Configure<TestOptions>(options => options.Value = "Test");
        _services.AddFluentInjections<IApplicationBuilder>(typeof(TestMiddlewareModule).Assembly);

        // Build the service provider
        _provider = _services.BuildServiceProvider();
        _appBuilder = _provider.GetRequiredService<IApplicationBuilder>();
    }

    [Fact]
    public async Task Middleware_Should_Be_RegisteredAsync()
    {
        // Arrange
        var middlewareExecuted = false;
        var configurator = new MiddlewareConfigurator<IApplicationBuilder>(_services, _appBuilder);

        configurator.UseMiddleware<TestMiddleware>(); // Use the generic middleware with options
        configurator.Register((descriptor, context, builder) =>
        {
            middlewareExecuted = true;
        });

        // Act
        await _appBuilder.RunPipelineAsync();

        // Assert
        Assert.True(middlewareExecuted, "Middleware was not executed in the pipeline.");
        Assert.Single(TestMiddleware<TestOptions>.CallOrder, "TestMiddleware was not executed as expected.");
    }

    private async Task RunPipeline()
    {
        using (var scope = _provider.CreateScope())
        {
            var context = new DefaultHttpContext();
            var app = scope.ServiceProvider.GetRequiredService<IApplicationBuilder>();
            var pipeline = app.Build();
            await pipeline.Invoke(context);
        }
    }
}

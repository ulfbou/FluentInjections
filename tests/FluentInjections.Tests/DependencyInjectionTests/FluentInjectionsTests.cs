using Autofac.Core;

using FluentInjections;
using FluentInjections.Internal.Registries;
using FluentInjections.Tests.Modules;
using FluentInjections.Tests.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using System.Reflection;

using Xunit;

namespace FluentInjections.Tests.DependencyInjectionTests;

public partial class FluentInjectionsTests : IDisposable
{
    private readonly WebApplicationBuilder _builder;
    private readonly IServiceCollection _services;
    private readonly WebApplication _app;
    private readonly AsyncServiceScope _scope;
    private readonly IServiceProvider _provider;
    private bool disposedValue;

    public FluentInjectionsTests()
    {
        _builder = WebApplication.CreateBuilder();
        _builder.Services.AddFluentInjections<WebApplicationBuilder, ModuleRegistry<WebApplicationBuilder>>(typeof(InjectionTestServiceModule).Assembly);
        _services = _builder.Services;
        _app = _builder.Build();
        _scope = _app.Services.CreateAsyncScope();
        _provider = _scope.ServiceProvider;
    }

    [Fact]
    public void AddFluentInjections_Should_RegisterServiceModule()
    {
        // Arrange & Act
        var service = _provider.GetService<ITestService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void AddFluentInjections_Should_RegisterServiceModule_With_Parameters()
    {
        // Arrange
        // Act
        var service = _provider.GetNamedService<ITestService>("Test43");

        // Assert
        Assert.NotNull(service);
        var serviceInstance = Assert.IsType<TestService>(service);
        Assert.Equal("value1", serviceInstance.Param1);
        Assert.Equal(42, serviceInstance.Param2);
    }

    [Fact]
    public void UseFluentInjections_Should_RegisterMiddlewareModule()
    {
        // Arrange
        var mockApp = new Mock<IApplicationBuilder>();
        var app = mockApp.Object;

        // Act
        app.UseFluentInjections(Assembly.GetExecutingAssembly());

        // Assert
        mockApp.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _scope.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~FluentInjectionsTests()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

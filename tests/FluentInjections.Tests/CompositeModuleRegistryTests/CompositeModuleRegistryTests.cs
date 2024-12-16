using Xunit;
using Microsoft.Extensions.DependencyInjection;
using FluentInjections;
using System;
using System.Collections.Concurrent;
using Moq;
using Microsoft.AspNetCore.Builder;
using FluentInjections.Internal.Registries;

namespace FluentInjections.Tests;

public class CompositeModuleRegistryTests
{
    public class TestServiceModule : IServiceModule
    {
        public void ConfigureServices(IServiceConfigurator configurator)
        {
            // Implement the service configuration logic here
        }
    }

    public class TestMiddlewareModule : IMiddlewareModule<IApplicationBuilder>
    {
        public void ConfigureMiddleware(IMiddlewareConfigurator<IApplicationBuilder> configurator)
        {
            // Implement the middleware configuration logic here
        }
    }

    [Fact]
    public void AddRegistry_AddsRegistrySuccessfully()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var compositeRegistry = new CompositeModuleRegistry<IApplicationBuilder>(serviceProvider);
        var mockRegistry = new Mock<IModuleRegistry<IApplicationBuilder>>();

        compositeRegistry.AddRegistry(mockRegistry.Object);

        var registriesField = typeof(CompositeModuleRegistry<IApplicationBuilder>).GetField("_registries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var registries = registriesField?.GetValue(compositeRegistry) as ConcurrentBag<IModuleRegistry<IApplicationBuilder>>;

        Assert.NotNull(registries);
        Assert.Single(registries);
        Assert.Contains(mockRegistry.Object, registries);
    }

    [Fact]
    public void RegisterModule_ServiceModule_RegistersCorrectly()
    {
        var services = new ServiceCollection();
        var mockRegistry = new Mock<IModuleRegistry<IApplicationBuilder>>();
        var moduleRegistry = new ModuleRegistry<IApplicationBuilder>();
        services.AddSingleton(mockRegistry.Object);
        services.AddSingleton(moduleRegistry);
        var serviceProvider = services.BuildServiceProvider();
        var compositeRegistry = new CompositeModuleRegistry<IApplicationBuilder>(serviceProvider);
        var serviceModule = new TestServiceModule();

        mockRegistry.Setup(r => r.CanHandle(serviceModule.GetType())).Returns(true);
        mockRegistry.Setup(r => r.RegisterModule(serviceModule)).Returns(mockRegistry.Object);

        compositeRegistry.RegisterModule(serviceModule);

        mockRegistry.Verify(r => r.RegisterModule(serviceModule), Times.Once);
    }

    [Fact]
    public void RegisterModule_MiddlewareModule_RegistersCorrectly()
    {
        var services = new ServiceCollection();
        var mockRegistry = new Mock<IModuleRegistry<IApplicationBuilder>>();
        var moduleRegistry = new ModuleRegistry<IApplicationBuilder>();
        services.AddSingleton(mockRegistry.Object);
        services.AddSingleton(moduleRegistry);
        var serviceProvider = services.BuildServiceProvider();
        var compositeRegistry = new CompositeModuleRegistry<IApplicationBuilder>(serviceProvider);
        var middlewareModule = new TestMiddlewareModule();

        mockRegistry.Setup(r => r.CanHandle(middlewareModule.GetType())).Returns(true);
        mockRegistry.Setup(r => r.RegisterModule(middlewareModule)).Returns(mockRegistry.Object);

        compositeRegistry.RegisterModule(middlewareModule);

        mockRegistry.Verify(r => r.RegisterModule(middlewareModule), Times.Once);
    }

    [Fact]
    public void RegisterModule_WithFactory_RegistersCorrectly()
    {
        var services = new ServiceCollection();
        var mockRegistry = new Mock<IModuleRegistry<IApplicationBuilder>>();
        var moduleRegistry = new ModuleRegistry<IApplicationBuilder>();
        services.AddSingleton(mockRegistry.Object);
        services.AddSingleton(moduleRegistry);
        var serviceProvider = services.BuildServiceProvider();
        var compositeRegistry = new CompositeModuleRegistry<IApplicationBuilder>(serviceProvider);

        mockRegistry.Setup(r => r.CanHandle(typeof(TestServiceModule))).Returns(true);
        mockRegistry.Setup(r => r.RegisterModule(It.IsAny<Func<TestServiceModule>>(), It.IsAny<Action<TestServiceModule>>())).Returns(mockRegistry.Object);

        compositeRegistry.RegisterModule(() => new TestServiceModule());

        mockRegistry.Verify(r => r.RegisterModule(It.IsAny<Func<TestServiceModule>>(), It.IsAny<Action<TestServiceModule>>()), Times.Once);
    }

    [Fact]
    public void RegisterModule_WithCondition_RegistersCorrectly()
    {
        var services = new ServiceCollection();
        var mockRegistry = new Mock<IModuleRegistry<IApplicationBuilder>>();
        var moduleRegistry = new ModuleRegistry<IApplicationBuilder>();
        services.AddSingleton(mockRegistry.Object);
        services.AddSingleton(moduleRegistry);
        var serviceProvider = services.BuildServiceProvider();
        var compositeRegistry = new CompositeModuleRegistry<IApplicationBuilder>(serviceProvider);

        mockRegistry.Setup(r => r.CanHandle(typeof(TestServiceModule))).Returns(true);
        mockRegistry.Setup(r => r.RegisterModule<TestServiceModule>(It.IsAny<Func<bool>>())).Returns(mockRegistry.Object);

        compositeRegistry.RegisterModule<TestServiceModule>(() => true);

        mockRegistry.Verify(r => r.RegisterModule<TestServiceModule>(It.IsAny<Func<bool>>()), Times.Once);
    }

    [Fact]
    public void ApplyServiceModules_AppliesToAllRegistries()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var compositeRegistry = new CompositeModuleRegistry<IApplicationBuilder>(serviceProvider);
        var mockServiceConfigurator = new Mock<IServiceConfigurator>();
        var mockRegistry1 = new Mock<IModuleRegistry<IApplicationBuilder>>();
        var mockRegistry2 = new Mock<IModuleRegistry<IApplicationBuilder>>();

        compositeRegistry.AddRegistry(mockRegistry1.Object);
        compositeRegistry.AddRegistry(mockRegistry2.Object);

        compositeRegistry.ApplyServiceModules(mockServiceConfigurator.Object);

        mockRegistry1.Verify(r => r.ApplyServiceModules(mockServiceConfigurator.Object), Times.Once);
        mockRegistry2.Verify(r => r.ApplyServiceModules(mockServiceConfigurator.Object), Times.Once);
    }

    [Fact]
    public void ApplyMiddlewareModules_AppliesToAllRegistries()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var compositeRegistry = new CompositeModuleRegistry<IApplicationBuilder>(serviceProvider);
        var mockMiddlewareConfigurator = new Mock<IMiddlewareConfigurator<IApplicationBuilder>>();
        var mockRegistry1 = new Mock<IModuleRegistry<IApplicationBuilder>>();
        var mockRegistry2 = new Mock<IModuleRegistry<IApplicationBuilder>>();

        compositeRegistry.AddRegistry(mockRegistry1.Object);
        compositeRegistry.AddRegistry(mockRegistry2.Object);

        compositeRegistry.ApplyMiddlewareModules(mockMiddlewareConfigurator.Object);

        mockRegistry1.Verify(r => r.ApplyMiddlewareModules(mockMiddlewareConfigurator.Object), Times.Once);
        mockRegistry2.Verify(r => r.ApplyMiddlewareModules(mockMiddlewareConfigurator.Object), Times.Once);
    }

    [Fact]
    public void InitializeModules_InitializesAllRegistries()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var compositeRegistry = new CompositeModuleRegistry<IApplicationBuilder>(serviceProvider);
        var mockRegistry1 = new Mock<IModuleRegistry<IApplicationBuilder>>();
        var mockRegistry2 = new Mock<IModuleRegistry<IApplicationBuilder>>();

        compositeRegistry.AddRegistry(mockRegistry1.Object);
        compositeRegistry.AddRegistry(mockRegistry2.Object);

        compositeRegistry.InitializeModules();

        mockRegistry1.Verify(r => r.InitializeModules(), Times.Once);
        mockRegistry2.Verify(r => r.InitializeModules(), Times.Once);
    }
}

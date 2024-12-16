using FluentInjections.Internal.Registries;
using FluentInjections.Internal.Modules;
using FluentInjections;

using Microsoft.AspNetCore.Builder;

using Moq;

using Xunit;

namespace FluentInjections.Tests.ModuleRegistryTests;

public class ModuleRegistryTests
{
    private readonly ModuleRegistry<IApplicationBuilder> _moduleRegistry = new();

    [Fact]
    public void RegisterModule_NullModule_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => _moduleRegistry.RegisterModule((IServiceModule)null!));
    }

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
    public void RegisterModule_WithCondition_AddsModule()
    {
        var registry = new ModuleRegistry<IApplicationBuilder>();
        bool conditionMet = true;

        registry.RegisterModule<TestServiceModule>(() => conditionMet);

        var serviceModulesField = registry.GetType().GetField("_serviceModules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var serviceModules = serviceModulesField?.GetValue(registry) as List<IServiceModule>;

        Assert.NotNull(serviceModules);
        Assert.Single(serviceModules);
        Assert.IsType<TestServiceModule>(serviceModules[0]);
    }

    [Fact]
    public void RegisterModule_WithServiceModule_AddsModule()
    {
        var registry = new ModuleRegistry<IApplicationBuilder>();
        var module = new TestServiceModule();

        registry.RegisterModule(module);

        var serviceModulesField = registry.GetType().GetField("_serviceModules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var serviceModules = serviceModulesField?.GetValue(registry) as List<IServiceModule>;

        Assert.NotNull(serviceModules);
        Assert.Single(serviceModules);
        Assert.Equal(module, serviceModules[0]);
    }

    [Fact]
    public void RegisterModule_WithMiddlewareModule_AddsModule()
    {
        var registry = new ModuleRegistry<IApplicationBuilder>();
        var module = new TestMiddlewareModule();

        registry.RegisterModule(module);

        var middlewareModulesField = registry.GetType().GetField("_middlewareModules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var middlewareModules = middlewareModulesField?.GetValue(registry) as List<IMiddlewareModule<IApplicationBuilder>>;

        Assert.NotNull(middlewareModules);
        Assert.Single(middlewareModules);
        Assert.Equal(module, middlewareModules[0]);
    }

    [Fact]
    public void RegisterModule_WithFactory_AddsModule()
    {
        var registry = new ModuleRegistry<IApplicationBuilder>();
        var factoryModule = new TestServiceModule();

        registry.RegisterModule(() => factoryModule);

        var serviceModulesField = registry.GetType().GetField("_serviceModules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var serviceModules = serviceModulesField?.GetValue(registry) as List<IServiceModule>;

        Assert.NotNull(serviceModules);
        Assert.Single(serviceModules);
        Assert.IsType<LazyServiceModule>(serviceModules[0]);
    }

    [Fact]
    public void ApplyServiceModules_ConfiguresAllServiceModules()
    {
        var registry = new ModuleRegistry<IApplicationBuilder>();
        var serviceConfiguratorMock = new Mock<IServiceConfigurator>();
        var module1 = new Mock<IServiceModule>();
        var module2 = new Mock<IServiceModule>();

        registry.RegisterModule(module1.Object);
        registry.RegisterModule(module2.Object);

        registry.ApplyServiceModules(serviceConfiguratorMock.Object);

        module1.Verify(m => m.ConfigureServices(serviceConfiguratorMock.Object), Times.Once);
        module2.Verify(m => m.ConfigureServices(serviceConfiguratorMock.Object), Times.Once);
    }

    [Fact]
    public void ApplyMiddlewareModules_ConfiguresAllMiddlewareModules()
    {
        var registry = new ModuleRegistry<IApplicationBuilder>();
        var middlewareConfiguratorMock = new Mock<IMiddlewareConfigurator<IApplicationBuilder>>();
        var module1 = new Mock<IMiddlewareModule<IApplicationBuilder>>();
        var module2 = new Mock<IMiddlewareModule<IApplicationBuilder>>();

        registry.RegisterModule(module1.Object);
        registry.RegisterModule(module2.Object);

        registry.ApplyMiddlewareModules(middlewareConfiguratorMock.Object);

        module1.Verify(m => m.ConfigureMiddleware(middlewareConfiguratorMock.Object), Times.Once);
        module2.Verify(m => m.ConfigureMiddleware(middlewareConfiguratorMock.Object), Times.Once);
    }

    [Fact]
    public void InitializeModules_InitializesAllModules()
    {
        var registry = new ModuleRegistry<IApplicationBuilder>();
        var initializableModule1 = new Mock<IServiceModule>();
        var initializableModule2 = new Mock<IMiddlewareModule<IApplicationBuilder>>();

        initializableModule1.As<IInitializable>().Setup(m => m.Initialize()).Verifiable();
        initializableModule2.As<IInitializable>().Setup(m => m.Initialize()).Verifiable();

        registry.RegisterModule(initializableModule1.Object);
        registry.RegisterModule(initializableModule2.Object);

        registry.InitializeModules();

        initializableModule1.As<IInitializable>().Verify(m => m.Initialize(), Times.Once);
        initializableModule2.As<IInitializable>().Verify(m => m.Initialize(), Times.Once);
    }

    [Fact]
    public void CanHandle_InvalidModuleType_ThrowsException()
    {
        var registry = new ModuleRegistry<IApplicationBuilder>();

        Assert.Throws<InvalidRegistrationException>(() => registry.CanHandle(typeof(string)));
    }
}

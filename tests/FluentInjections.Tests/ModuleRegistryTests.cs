using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace FluentInjections.Tests
{
    public class ModuleRegistryTests
    {
        private class TestServiceModule : IServiceModule 
        {
            public void ConfigureServices(IServiceConfigurator configurator) { }
        }

        private class TestMiddlewareModule : IMiddlewareModule<IApplicationBuilder>
        {
            public void ConfigureMiddleware(IMiddlewareConfigurator<IApplicationBuilder> configurator) { }
        }

        [Fact]
        public void RegisterConditionalModule_AddsModule_WhenConditionIsTrue()
        {
            var registry = new ModuleRegistry<IApplicationBuilder>();
            registry.RegisterConditionalModule<TestServiceModule>(() => true);

            Assert.Single(registry._serviceModules);
            Assert.IsType<TestServiceModule>(registry._serviceModules[0]);
        }

        [Fact]
        public void RegisterModule_AddsServiceModule()
        {
            var registry = new ModuleRegistry<IApplicationBuilder>();
            var module = new TestServiceModule();

            registry.RegisterModule(module);

            Assert.Single(registry._serviceModules);
            Assert.Equal(module, registry._serviceModules[0]);
        }

        [Fact]
        public void RegisterModule_AddsMiddlewareModule()
        {
            var registry = new ModuleRegistry<IApplicationBuilder>();
            var module = new TestMiddlewareModule();

            registry.RegisterModule(module);

            Assert.Single(registry._middlewareModules);
            Assert.Equal(module, registry._middlewareModules[0]);
        }

        [Fact]
        public void ApplyServiceModules_CallsConfigureServicesAndValidate()
        {
            var registry = new ModuleRegistry<IApplicationBuilder>();
            var serviceConfigurator = new Mock<IServiceConfigurator>();

            var serviceModule = new Mock<IServiceModule>();
            serviceModule.Setup(m => m.ConfigureServices(It.IsAny<IServiceConfigurator>()));

            registry.RegisterModule(serviceModule.Object);
            registry.ApplyServiceModules(serviceConfigurator.Object);

            serviceModule.Verify(m => m.ConfigureServices(serviceConfigurator.Object), Times.Once);
        }

        [Fact]
        public void ApplyMiddlewareModules_CallsConfigureMiddlewareAndValidate()
        {
            var registry = new ModuleRegistry<IApplicationBuilder>();
            var middlewareConfigurator = new Mock<IMiddlewareConfigurator<IApplicationBuilder>>();

            var middlewareModule = new Mock<IMiddlewareModule<IApplicationBuilder>>();
            middlewareModule.Setup(m => m.ConfigureMiddleware(It.IsAny<IMiddlewareConfigurator<IApplicationBuilder>>()));

            registry.RegisterModule(middlewareModule.Object);
            registry.ApplyMiddlewareModules(middlewareConfigurator.Object);

            middlewareModule.Verify(m => m.ConfigureMiddleware(middlewareConfigurator.Object), Times.Once);
        }

        [Fact]
        public void InitializeModules_CallsInitializeOnInitializableModules()
        {
            var registry = new ModuleRegistry<IApplicationBuilder>();

            var initializableModule = new Mock<IServiceModule>();
            var initializable = initializableModule.As<IInitializable>();
            initializable.Setup(m => m.Initialize());

            registry.RegisterModule(initializableModule.Object);
            registry.InitializeModules();

            initializable.Verify(m => m.Initialize(), Times.Once);
        }

        [Fact]
        public void CanHandle_ThrowsInvalidRegistrationException()
        {
            var registry = new ModuleRegistry<IApplicationBuilder>();

            Assert.Throws<InvalidRegistrationException>(() => registry.CanHandle<TestServiceModule>());
        }
    }
}
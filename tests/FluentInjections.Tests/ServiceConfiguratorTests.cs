using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace FluentInjections.Tests
{
    public class ServiceConfiguratorTests
    {
        private class TestService : ITestService { }
        private class TestServiceImplementation : ITestService { }
        private class MyOptions { public string Option { get; set; } }

        [Fact]
        public void AddService_RegistersServiceWithLifetime()
        {
            var services = new ServiceCollection();
            var configurator = new ServiceConfigurator(services);

            configurator.AddService<ITestService, TestServiceImplementation>(ServiceLifetime.Transient);

            var serviceDescriptor = Assert.Single(services);
            Assert.Equal(typeof(ITestService), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(TestServiceImplementation), serviceDescriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
        }

        [Fact]
        public void AddSingleton_RegistersSingletonInstance()
        {
            var services = new ServiceCollection();
            var configurator = new ServiceConfigurator(services);
            var instance = new TestService();

            configurator.AddSingleton<ITestService>(instance);

            var serviceDescriptor = Assert.Single(services);
            Assert.Equal(typeof(ITestService), serviceDescriptor.ServiceType);
            Assert.Same(instance, serviceDescriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
        }

        [Fact]
        public void ConfigureOptions_ConfiguresOptionsCorrectly()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new MyOptions { Option = "Initial" });
            var configurator = new ServiceConfigurator(services);

            configurator.ConfigureOptions<MyOptions>(options => options.Option = "Configured");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<MyOptions>();
            Assert.Equal("Configured", options.Option);
        }

        [Fact]
        public void AddTransient_RegistersServiceWithTransientLifetime()
        {
            var services = new ServiceCollection();
            var configurator = new ServiceConfigurator(services);

            configurator.AddTransient<ITestService, TestServiceImplementation>();

            var serviceDescriptor = Assert.Single(services);
            Assert.Equal(typeof(ITestService), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(TestServiceImplementation), serviceDescriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
        }
    }

    public interface ITestService { }
}
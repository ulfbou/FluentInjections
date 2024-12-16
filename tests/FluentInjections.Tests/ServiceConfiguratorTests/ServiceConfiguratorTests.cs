using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace FluentInjections.Tests.ServiceConfiguratorTests;

public class ServiceConfiguratorTests
{
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly ServiceConfigurator _serviceConfigurator;

    public ServiceConfiguratorTests()
    {
        _serviceConfigurator = new ServiceConfigurator(_services);
    }

    [Fact]
    public void Bind_ServiceToImplementation_ShouldRegisterService()
    {
        // Arrange
        var binding = _serviceConfigurator.Bind<ITestService>().As<TestService>();

        // Act
        binding.Register();

        // Assert
        var serviceDescriptor = Assert.Single(_services);
        Assert.Equal(typeof(ITestService), serviceDescriptor.ServiceType);
        Assert.Equal(typeof(TestService), serviceDescriptor.ImplementationType);
    }

    [Fact]
    public void Bind_ServiceWithLifetime_ShouldRegisterServiceWithLifetime()
    {
        // Arrange
        var binding = _serviceConfigurator.Bind<ITestService>().As<TestService>().WithLifetime(ServiceLifetime.Singleton);

        // Act
        binding.Register();

        // Assert
        var serviceDescriptor = Assert.Single(_services);
        Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void Bind_ServiceWithParameters_ShouldRegisterServiceWithParameters()
    {
        // Arrange
        var parameters = new { Param1 = "value1", Param2 = 42 };
        var binding = _serviceConfigurator.Bind<ITestService>().As<TestService>().WithParameters(parameters);

        // Act
        binding.Register();

        // Assert
        var serviceDescriptor = Assert.Single(_services);
        Assert.Equal(typeof(ITestService), serviceDescriptor.ServiceType);
        Assert.Equal(typeof(TestService), serviceDescriptor.ImplementationType);
    }

    [Fact]
    public void Bind_ServiceAsSelf_ShouldRegisterServiceAsSelf()
    {
        // Arrange
        var binding = _serviceConfigurator.Bind<TestService>().AsSelf();

        // Act
        binding.Register();

        // Assert
        var serviceDescriptor = Assert.Single(_services);
        Assert.Equal(typeof(TestService), serviceDescriptor.ServiceType);
        Assert.Equal(typeof(TestService), serviceDescriptor.ImplementationType);
    }

    [Fact]
    public void Bind_ServiceWithInstance_ShouldRegisterServiceWithInstance()
    {
        // Arrange
        var instance = new TestService();
        var binding = _serviceConfigurator.Bind<ITestService>().WithInstance(instance);

        // Act
        binding.Register();

        // Assert
        var serviceDescriptor = Assert.Single(_services);
        Assert.Equal(instance, serviceDescriptor.ImplementationInstance);
    }

    [Fact]
    public void Bind_ServiceWithFactory_ShouldRegisterServiceWithFactory()
    {
        // Arrange
        Func<ITestService> factory = () => new TestService();
        var binding = _serviceConfigurator.Bind<ITestService>().WithFactory(factory);

        // Act
        binding.Register();

        // Assert
        var serviceDescriptor = Assert.Single(_services);
        Assert.Equal(typeof(ITestService), serviceDescriptor.ServiceType);
    }

    [Fact]
    public void Bind_ServiceWithConfigure_ShouldInvokeConfigureAction()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        var binding = _serviceConfigurator.Bind<ITestService>().WithInstance(mockService.Object).Configure(service => service.DoSomething());

        // Act
        binding.Register();

        // Assert
        mockService.Verify(service => service.DoSomething(), Times.Once);
    }

    [Fact]
    public void Bind_ServiceWithConfigureOptions_ShouldConfigureOptions()
    {
        // Arrange
        var options = new TestOptions();
        var binding = _serviceConfigurator.Bind<ITestService>().ConfigureOptions<TestOptions>(opts => opts.Option1 = "value");

        // Act
        binding.Register();

        // Assert
        Assert.Equal("value", options.Option1);
    }

    [Fact]
    public void Register_ServiceWithoutImplementation_ShouldThrowException()
    {
        // Arrange
        var binding = _serviceConfigurator.Bind<ITestService>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => binding.Register());
    }

    public interface ITestService
    {
        void DoSomething();
    }

    public sealed class TestService : ITestService
    {
        public void DoSomething()
        {
            // Implementation
        }
    }

    public sealed class TestOptions
    {
        public string Option1 { get; set; }
    }

}

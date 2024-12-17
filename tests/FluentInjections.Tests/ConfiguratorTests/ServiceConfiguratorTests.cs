using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

namespace FluentInjections.Tests.ConfiguratorTests;

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
        var binding = _serviceConfigurator.Bind<ITestService>().To<TestService>();

        // Act
        binding.Register();

        // Assert
        var serviceDescriptor = Assert.Single(_services);
        Assert.Equal(typeof(ITestService), serviceDescriptor.ServiceType);
    }

    [Fact]
    public void Bind_ServiceWithLifetime_ShouldRegisterServiceWithLifetime()
    {
        // Arrange
        var binding = _serviceConfigurator.Bind<ITestService>().To<TestService>().WithLifetime(ServiceLifetime.Singleton);

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
        Dictionary<string, object> parameters = new() { { "Param1", "value1" }, { "Param2", 42 } };
        var binding = _serviceConfigurator.Bind<ITestService>().To<TestService>().WithParameters(parameters.AsReadOnly());

        // Act
        binding.Register();

        // Assert
        var serviceDescriptor = Assert.Single(_services);
        Assert.Equal(typeof(ITestService), serviceDescriptor.ServiceType);
    }

    // Verify that a service registered with parameters are instantiated correctly by the DI container. 
    [Fact]
    public void Bind_ServiceWithParameters_ShouldInstantiateServiceWithParameters()
    {
        // Arrange
        Dictionary<string, object> parameters = new() { { "Param1", "value1" }, { "Param2", 42 } };
        var binding = _serviceConfigurator.Bind<ITestService>().To<TestService>().WithParameters(parameters.AsReadOnly());

        // Act
        binding.Register();

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var service = serviceProvider.GetRequiredService<ITestService>() as TestService;
        Assert.NotNull(service);
        Assert.Equal("value1", service.Param1);
        Assert.Equal(42, service.Param2);
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
        Func<IServiceProvider, ITestService> factory = sp => new TestService();
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
    public void Register_ServiceWithoutImplementation_ShouldThrowException()
    {
        // Arrange
        var binding = _serviceConfigurator.Bind<ITestService>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => binding.Register());
    }

    [Fact]
    public void Bind_ServiceWithConfigureOptions_ShouldConfigureOptions()
    {
        // Arrange
        var binding = _serviceConfigurator.Bind<TestService>().AsSelf().ConfigureOptions<TestOptions>(opts => opts.Option1 = "value");

        // Act
        binding.Register();
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TestOptions>>().Value;

        // Assert
        Assert.Equal("value", options.Option1);
    }

    public interface ITestService
    {
        void DoSomething();
    }

    public sealed class TestService : ITestService
    {
        public string? Param1 { get; set; }
        public int Param2 { get; set; }

        public TestService(string? param1 = null, int? param2 = null)
        {
            Param1 = param1;
            Param2 = param2.HasValue ? param2.Value : 0;
        }

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

﻿
using Autofac;
using Autofac.Core;

using FluentAssertions;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Registries;
using FluentInjections.Tests.Modules;
using FluentInjections.Tests.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

namespace FluentInjections.Tests.ConfiguratorTests;

public class ServiceConfiguratorTests : BaseTest
{
    public ServiceConfiguratorTests() : base() { }

    [Fact]
    public void Bind_ServiceWithParameters_ShouldInstantiateServiceWithParameters()
    {
        // Arrange
        Dictionary<string, object> parameters = new() { { "param1", "value1" }, { "param2", 42 } };
        RegisterService<ITestService>(binding =>
        {
            binding.To<TestService>()
                   .WithParameters(parameters.AsReadOnly());
        });

        // Act
        Register();
        var service = Resolve<ITestService>() as TestService;

        // Assert
        service.Should().NotBeNull();
        service!.Param1.Should().Be("value1");
        service.Param2.Should().Be(42);
    }

    [Fact]
    public void Bind_ServiceToImplementation_ShouldRegisterService()
    {
        // Arrange
        RegisterService<ITestService>(binding =>
        {
            binding.To<TestService>();
        });

        // Act
        Register();
        var service = Resolve<ITestService>() as TestService;

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Bind_ServiceWithLifetime_ShouldRegisterServiceWithLifetime()
    {
        // Arrange
        RegisterService<ITestService>(binding =>
        {
            binding.To<TestService>()
                   .AsSingleton();
        });

        // Act
        Register();
        var service1 = Resolve<ITestService>() as TestService;
        var service2 = Resolve<ITestService>() as TestService;

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public void Bind_ServiceAsSelf_ShouldRegisterServiceAsSelf()
    {
        // Arrange
        RegisterService<TestService>(binding =>
        {
            binding.AsSelf();
        });

        // Act
        Register();
        var service = Resolve<TestService>();

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Bind_ServiceWithInstance_ShouldRegisterServiceWithInstance()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        RegisterService<ITestService>(binding =>
        {
            binding.WithInstance(mockService.Object);
        });

        // Act
        Register();
        var service = Resolve<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().Be(mockService.Object);
    }

    [Fact]
    public void Bind_ServiceWithFactory_ShouldRegisterServiceWithFactory()
    {
        // Arrange
        RegisterService<ITestService>(binding =>
        {
            binding.WithFactory(sp => new TestService("value1", 42));
        });

        // Act
        Register();
        var service = Resolve<ITestService>() as TestService;

        // Assert
        service.Should().NotBeNull();
        service!.Param1.Should().Be("value1");
        service.Param2.Should().Be(42);
    }

    [Fact]
    public void Bind_ServiceWithConfigure_ShouldInvokeConfigureAction()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        RegisterService<ITestService>(binding =>
        {
            binding.To<TestService>()
                   .Configure(service => service.DoSomething());
        });

        // Act
        Register();
        var service = Resolve<ITestService>() as TestService;

        // Assert
        service.Should().NotBeNull();
        service!.Param1.Should().Be("something");
    }

    [Fact]
    public void Register_ServiceWithoutImplementation_ShouldThrowException()
    {
        // Arrange & Act
        Action act = () => RegisterService<ITestService>(binding =>
        {
            binding.To<ITestService>();
        });

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_ServiceWithMultipleParameters_RegistersCorrectly()
    {
        // Arrange
        RegisterService<ComplexService>(binding =>
        {
            binding.To<ComplexService>()
                   .WithParameters(new { param1 = "value1", param2 = 42 });
        });

        // Act
        Register();
        var service = Resolve<ComplexService>();

        // Assert
        service.Should().NotBeNull();
        service!.Param1.Should().Be("value1");
        service.Param2.Should().Be(42);
    }

    [Fact]
    public void Bind_ServiceWithInvalidParameters_ThrowsArgumentException()
    {
        // Arrange
        RegisterService<ComplexService>(binding =>
        {
            binding.To<ComplexService>()
                   .WithParameters(new { param1 = "value1" });
        });

        // Act
        Register();
        Action act = () => Resolve<ComplexService>();

        // Assert
        act.Should().Throw<DependencyResolutionException>();
    }

    [Fact]
    public void Bind_ExistingService_ReplacesPreviousRegistration()
    {
        // Arrange
        RegisterService<ITestService>(binding =>
        {
            binding.To<TestService>();
        });
        RegisterService<ITestService>(binding =>
        {
            binding.To<TestServiceWithDefaultValues>();
        });

        // Act
        Register();
        var service = Resolve<ITestService>() as TestServiceWithDefaultValues;

        // Assert
        service.Should().NotBeNull();
        service!.Param1.Should().Be("default");
        service.Param2.Should().Be(-1);
    }

    //[Fact]
    //public void Bind_ServiceWithConfigureOptions_ShouldConfigureOptions()
    //{
    //    // Arrange
    //    var configurator = new ServiceConfigurator(_services);
    //    var binding = configurator.Bind<TestService>().AsSelf().Configure<TestServiceOptions>(opts => opts.Option1 = "value");

    //    // Act
    //    configurator.Register();
    //    var serviceProvider = _services.BuildServiceProvider();
    //    var options = serviceProvider.GetRequiredService<IOptions<TestServiceOptions>>().Value;

    //    // Assert
    //    Assert.Equal("value", options.Option1);
    //}

}

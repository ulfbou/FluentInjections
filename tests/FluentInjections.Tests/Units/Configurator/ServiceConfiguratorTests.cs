using FluentAssertions;

using FluentInjections.Extensions;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Internal.Services;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using static FluentInjections.Internal.Configurators.ServiceConfigurator;

namespace FluentInjections.Tests.Units.Configurator;

public abstract class ServiceConfiguratorTests<TConfigurator, TService, TFixture> : ConfiguratorTests<TConfigurator, TService, TFixture>
    where TConfigurator : class, IServiceConfigurator
    where TService : class
    where TFixture : class, IServiceConfiguratorFixture<TConfigurator, TService>, new()
{
    public ServiceConfiguratorTests() : base()
    {
        Services = Fixture.Services;
        Configurator = Fixture.Configurator;
    }

    private readonly Mock<ITestService> _mockService = new();
    protected abstract void BuildProvider();

    [Fact]
    public void Register_WithImplementationType_RegistersType()
    {
        // Arrange
        var binding = Configurator.Bind<ITestService>()
                                  .To<TestService>()
                                  as ServiceBinding<ITestService>;

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void Register_WithInstance_RegistersInstance()
    {
        // Arrange
        var binding = Configurator.Bind<ITestService>()
                                  .WithInstance(_mockService.Object);

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeSameAs(_mockService.Object);
    }

    [Fact]
    public void Register_WithFactory_RegistersFactory()
    {
        // Arrange
        Configurator.Bind<ITestService>()
                    .WithFactory(provider => _mockService.Object);

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeSameAs(_mockService.Object);
    }

    [Fact]
    public void Register_WithConfigure_CallsConfigure()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        Configurator.Bind<ITestService>()
                    .WithFactory(provider => mockService.Object)
                    .Configure(descriptor => mockService.Setup(service => service.Param2).Returns(42));

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeSameAs(mockService.Object);
        service.Param2.Should().Be(42);
        mockService.Verify(service => service.Param2, Times.Once);
    }

    [Fact]
    public void Register_WithKey_RegistersKeyedTestService()
    {
        // Arrange
        var binding = Configurator.Bind<ITestService>()
                                  .To<TestService>()
                                  .WithKey("key")
                                  as ServiceBinding<ITestService>;
        var descriptor = binding?.GetDescriptor();

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredNamedService<ITestService>("key");

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void Register_WithKeyAndFactory_RegistersKeyedFactory()
    {
        // Arrange
        Configurator.Bind<ITestService>()
                    .WithKey("key")
                    .WithFactory(provider => _mockService.Object);

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredNamedService<ITestService>("key");

        // Assert
        service.Should().NotBeNull();
        service.Should().BeSameAs(_mockService.Object);
    }

    [Fact]
    public void Register_WithMetadata_RegistersMetadata()
    {
        // Arrange
        Configurator.Bind<ITestService>()
                    .To<TestService>()
                    .WithMetadata("param1", "value1")
                    .WithMetadata("param2", 42);

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void Register_MultipleImplementations_ResolvesCorrectly()
    {
        Configurator.Bind<ITestService>().To<TestService>().WithKey("Service1");
        Configurator.Bind<ITestService>().To<AnotherTestService>().WithKey("Service2");

        Configurator.Register();
        BuildProvider();

        var service1 = GetRequiredNamedService<ITestService>("Service1");
        var service2 = GetRequiredNamedService<ITestService>("Service2");

        service1.Should().NotBeNull();
        service1.Should().BeOfType<TestService>();

        service2.Should().NotBeNull();
        service2.Should().BeOfType<AnotherTestService>();
    }

    [Fact]
    public void Register_DuplicateRegistrations_UsesLatest()
    {
        Configurator.Bind<ITestService>().To<TestService>();
        Configurator.Bind<ITestService>().To<AnotherTestService>();

        Configurator.Register();
        BuildProvider();

        var service = GetRequiredService<ITestService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<AnotherTestService>();
    }

    [Fact]
    public void Register_ScopedService_CreatesNewInstancePerScope()
    {
        Configurator.Bind<ITestService>().To<TestService>().AsScoped();

        Configurator.Register();
        BuildProvider();

        using (var scope = Provider!.CreateScope())
        {
            var service1 = scope.ServiceProvider.GetService<ITestService>();
            var service2 = scope.ServiceProvider.GetService<ITestService>();

            service1.Should().NotBeSameAs(service2);
        }
    }

    [Fact]
    public void Register_MergeDescriptors_MergesMetadataAndParameters()
    {
        // Act
        Configurator.Bind<ITestService>()
                    .To<TestService>()
                    .WithMetadata("key1", "value1")
                    .WithParameter("param1", "value1");

        Configurator.Bind<ITestService>()
                    .To<TestService>()
                    .WithMetadata("key2", "value2")
                    .WithParameter("param2", "value2");

        Configurator.ConflictResolution = ConflictResolutionMode.Merge;

        Configurator.Register();
        BuildProvider();
        var configurator = Configurator as ServiceConfigurator;
        var descriptor = configurator?.TryGetDescriptor(typeof(ITestService));

        descriptor.Should().NotBeNull();
        descriptor!.Metadata.Should().ContainKey("key1").And.ContainValue("value1");
        descriptor.Metadata.Should().ContainKey("key2").And.ContainValue("value2");
        descriptor.Parameters.Should().ContainKey("param1").And.ContainValue("value1");
        descriptor.Parameters.Should().ContainKey("param2").And.ContainValue("value2");
    }
}

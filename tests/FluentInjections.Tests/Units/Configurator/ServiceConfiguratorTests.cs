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

public abstract class ServiceConfiguratorTests<TConfigurator, TContainer, TFixture> : ConfiguratorTests<TConfigurator, TContainer, TFixture>
    where TConfigurator : class, IServiceConfigurator
    where TContainer : class
    where TFixture : class, IServiceConfiguratorFixture<TConfigurator, TContainer>, new()
{
    private readonly Mock<ITestService> _mockService = new();

    protected abstract void BuildProvider();
    protected abstract IReadOnlyDictionary<string, object> GetMetadata<TService>(string name) where TService : class;

    [Fact]
    public void Register_WithImplementationType_RegistersType()
    {
        // Arrange
        var binding = Configurator.Bind<ITestService>()
                                  .To<TestService>() as ServiceBinding<ITestService>;

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
    public void Register_WithName_RegistersnameedTestService()
    {
        // Arrange
        var binding = Configurator.Bind<ITestService>()
                                  .To<TestService>()
                                  .WithName("name")
                                  as ServiceBinding<ITestService>;
        var descriptor = binding?.GetDescriptor();

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredNamedService<ITestService>("name");

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void Register_WithNameAndFactory_RegistersnameedFactory()
    {
        // Arrange
        Configurator.Bind<ITestService>()
                    .WithName("name")
                    .WithFactory(provider => _mockService.Object);

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredNamedService<ITestService>("name");

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
        Configurator.Bind<ITestService>().To<TestService>().WithName("Service1");
        Configurator.Bind<ITestService>().To<AnotherTestService>().WithName("Service2");

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
    public void Register_ScopedService_ReturnsSameInstanceWithinScope()
    {
        Configurator.Bind<ITestService>().To<TestService>().AsScoped();

        Configurator.Register();
        BuildProvider();

        using (var scope = Provider!.CreateScope())
        {
            var service1 = scope.ServiceProvider.GetService<ITestService>();
            var service2 = scope.ServiceProvider.GetService<ITestService>();

            service1.Should().BeSameAs(service2);
        }
    }

    [Fact]
    public void Register_ScopedService_ReturnsDifferentInstancesWithNewScope()
    {
        Configurator.Bind<ITestService>().To<TestService>().AsScoped();

        Configurator.Register();
        BuildProvider();

        using (var scope = Provider!.CreateScope())
        {
            var service1 = scope.ServiceProvider.GetService<ITestService>();

            using (var newScope = Provider!.CreateScope())
            {
                var service2 = newScope.ServiceProvider.GetService<ITestService>();
                service1.Should().NotBeSameAs(service2);
            }
        }
    }

    [Fact]
    public void Register_MergeDescriptors_MergesMetadataAndParameters()
    {
        // Act
        Configurator.Bind<ITestService>()
                    .To<TestService>()
                    .WithName("MetadataTest")
                    .WithMetadata("name1", "value1")
                    .WithParameter("param1", "value1");

        Configurator.Bind<ITestService>()
                    .To<TestService>()
                    .WithName("MetadataTest")
                    .WithMetadata("name2", "value2")
                    .WithParameter("param2", "value2");

        Configurator.ConflictResolution = ConflictResolutionMode.Merge;

        Configurator.Register();
        BuildProvider();
        var configurator = Configurator as ServiceConfigurator;
        var descriptor = configurator?.TryGetDescriptor(typeof(ITestService));
        var metadata = GetMetadata<ITestService>("MetadataTest");

        descriptor.Should().NotBeNull();
        metadata.Should().ContainKey("name1").And.ContainValue("value1");
        metadata.Should().ContainKey("name2").And.ContainValue("value2");
        descriptor!.Parameters.Should().ContainKey("param1").And.ContainValue("value1");
        descriptor.Parameters.Should().ContainKey("param2").And.ContainValue("value2");
    }
}

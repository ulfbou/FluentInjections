// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using FluentAssertions.Common;

using FluentInjections.Extensions;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Internal.Configurators;
using FluentInjections.Tests.Internal.Middlewares;
using FluentInjections.Tests.Internal.Services;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using System.Reflection;

using static FluentInjections.Internal.Configurators.ServiceConfigurator;

namespace FluentInjections.Tests.Units.Configurator;

public abstract class ServiceConfiguratorTests<TConfigurator, TServices, TProvider, TFixture>
    : ConfiguratorTests<TConfigurator, TServices, TProvider, TFixture>
    where TConfigurator : class, ITestServiceConfigurator
    where TServices : class, IServiceCollection, new()
    where TProvider : class, IServiceProvider
    where TFixture : class, IServiceConfiguratorFixture<TConfigurator, TServices, TProvider>, IConfiguratorFixture<TConfigurator, TServices, TProvider>, new()
{
    protected abstract IReadOnlyDictionary<string, object?> GetMetadata<TService>(string name) where TService : class;
    protected abstract IReadOnlyDictionary<string, object?> GetMetadata<TService>() where TService : class;

    [Fact]
    public void Bind_ServiceType_ToImplementationType_RegistersType()
    {
        // Arrange
        var binding = Configurator.Bind<ITestService>()
                                  .To<TestService>() as ServiceBindingBuilder<ITestService>;

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void Bind_ServiceType_WithInstance_ResolvesInstanceCorrectly()
    {
        // Arrange
        var binding = Configurator.Bind<ITestService>()
                                  .WithInstance(MockTestService.Object);

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeSameAs(MockTestService.Object);
    }

    [Fact]
    public void Bind_ServiceType_WithFactory_ResolvesInstanceCorrectly()
    {
        // Arrange
        Configurator.Bind<ITestService>()
                    .WithFactory(provider => MockTestService.Object);

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredService<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeSameAs(MockTestService.Object);
    }

    [Fact]
    public void Bind_ServiceType_WithFactory_CallsConfigure_ResolvesInstanceWithPropertiesCorrectly()
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
    public void Bind_ServiceType_WithName_ResolvesNamedImplementationCorrectly()
    {
        // Arrange
        Configurator.Bind<ITestService>()
                    .To<TestService>()
                    .WithName("name");
        Configurator.Bind<ITestService>()
                    .To<AnotherTestService>()
                    .WithName("another");

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredNamedService<ITestService>("name");

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void Bind_ServiceType_WithNameAndFactory_ResolvesNamedInstanceCorrectly()
    {
        // Arrange
        Configurator.Bind<ITestService>()
                    .WithName("name")
                    .WithFactory(provider => MockTestService.Object);

        // Act
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredNamedService<ITestService>("name");

        // Assert
        service.Should().NotBeNull();
        service.Should().BeSameAs(MockTestService.Object);
    }

    [Fact]
    public void Bind_ServiceType_WithMetadata_ResolvesInstanceAndMetadataCorrectly()
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
        var metadata = GetMetadata<ITestService>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
        metadata.Should().ContainKey("param1").And.ContainValue("value1");
        metadata.Should().ContainKey("param2").And.ContainValue(42);
    }

    [Fact]
    public void Bind_ServiceType_ToMultipleNamedImplementations_ResolvesCorrectly()
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
    public void Bind_ServiceType_ToDuplicateRegistrations_UsesLatest()
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
    public void Bind_ServiceType_ScopedService_ReturnsSameInstanceWithinScope()
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
    public void Bind_ServiceType_ScopedService_ReturnsDifferentInstancesWithNewScope()
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
    public void Bind_ServiceType_MergeDescriptors_MergesMetadataAndParameters()
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
        var descriptor = configurator?.TryGetDescriptor<ITestService>("MetadataTest");
        var metadata = GetMetadata<ITestService>("MetadataTest");

        descriptor.Should().NotBeNull();
        metadata.Should().ContainKey("name1").And.ContainValue("value1");
        metadata.Should().ContainKey("name2").And.ContainValue("value2");
        descriptor!.Parameters.Should().ContainKey("param1").And.ContainValue("value1");
        descriptor.Parameters.Should().ContainKey("param2").And.ContainValue("value2");
    }

    [Fact]
    public void Bind_ServiceType_ToNull_Should_ThrowArgumentNullException()
    {
        // Act
        void action() => Configurator.Bind<ITestService>()
                                     .To(null!);
        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void Bind_ServiceType_WithFactoryNull_Should_ThrowArgumentNullException()
    {
        // Act
        void action() => Configurator.Bind<ITestService>()
                                     .WithFactory(null!);
        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void Bind_ServiceType_WithNameNull_Should_ThrowArgumentNullException()
    {
        // Act
        void action() => Configurator.Bind<ITestService>()
                                     .WithName(null!);
        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Bind_ServiceType_WithParameterKeyNull_Should_ThrowArgumentNullException()
    {
        // Act
        void action() => Configurator.Bind<ITestService>()
                                     .WithParameter(null!, "value");
        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Bind_ServiceType_WithParameterNull_Should_ThrowArgumentNullException()
    {
        // Act
        void action() => Configurator.Bind<ITestService>()
                                     .WithMetadata(null!, "value");
        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Prevent_WithDuplicateRegistrations_Should_ThrowInvalidOperationException()
    {
        // Arrange
        Configurator.ConflictResolution = ConflictResolutionMode.Prevent;

        // Act
        Configurator.Bind<ITestService>().To<TestService>();
        Configurator.Bind<ITestService>().To<TestService>();

        // Assert
        Assert.Throws<InvalidOperationException>(() => Configurator.Register());
    }


    [Fact]
    public void Bind_ServiceType_WithPreventConflictResolution_Should_NotThrowException()
    {
        // Act
        Configurator.ConflictResolution = ConflictResolutionMode.Prevent;
        Configurator.Bind<ITestService>().To<TestService>();
        Configurator.Bind<ITestService>().To<TestService>();

        // Assert
        Assert.Throws<InvalidOperationException>(() => Configurator.Register());
    }

    [Fact]
    public void Bind_ServiceType_WithIgnoreConflictResolution_Should_NotThrowException()
    {
        // Act
        Configurator.Bind<ITestService>().To<TestService>();
        Configurator.Bind<ITestService>().To<TestService>();
        Configurator.ConflictResolution = ConflictResolutionMode.Ignore;

        // Assert
        Configurator.Register();
    }

#if OPEN_GENERIC_SUPPORTED
    [Fact]
    public void Bind_GenericServiceType_ToGenericType_Should_ResolveCorrectly()
    {
        // Act
        Configurator.Bind(typeof(IGenericService<>)).To(typeof(GenericService<>));
        Configurator.Register();
        BuildProvider();
        var service = GetRequiredService<IGenericService<int>>();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }
#endif

    [Fact]
    public void WarnAndReplace_DuplicateRegistrations_Should_LogWarningAndReplace()
    {
        // Arrange
        Configurator.ConflictResolution = ConflictResolutionMode.WarnAndReplace;
        Configurator.Bind<ITestService>().To<TestService>();
        Configurator.Bind<ITestService>().To<AnotherTestService>();

        // Act
        Configurator.Register();

        // Assert
        Fixture.LoggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
        var descriptors = Configurator.GetDescriptors();
        Assert.Single(descriptors);
    }

    [Fact]
    public void Merge_DuplicateRegistrations_Should_Merge()
    {
        // Arrange
        Configurator.ConflictResolution = ConflictResolutionMode.Merge;
        Configurator.Bind<ITestService>().To<TestService>();
        Configurator.Bind<ITestService>().To<AnotherTestService>();

        // Act
        Configurator.Register();

        // Assert
        Fixture.LoggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Once);
        var descriptors = Configurator.GetDescriptors();
        Assert.Single(descriptors);
    }

    [Fact]
    public void Ignore_DuplicateRegistrations_Should_Ignore()
    {
        // Arrange
        Configurator.ConflictResolution = ConflictResolutionMode.Ignore;
        Configurator.Bind<ITestService>().To<TestService>();
        Configurator.Bind<ITestService>().To<AnotherTestService>();

        // Act
        Configurator.Register();

        // Assert
        var descriptors = Configurator.GetDescriptors();
        Assert.Equal(2, descriptors.Count());
    }

    [Fact]
    public void Replace_DuplicateRegistrations_Should_Replace()
    {
        // Arrange
        Configurator.ConflictResolution = ConflictResolutionMode.Replace;
        Configurator.Bind<ITestService>().To<TestService>();
        Configurator.Bind<ITestService>().To<AnotherTestService>();

        // Act
        Configurator.Register();

        // Assert
        var descriptors = Configurator.GetDescriptors();
        Assert.Single(descriptors);
    }
}

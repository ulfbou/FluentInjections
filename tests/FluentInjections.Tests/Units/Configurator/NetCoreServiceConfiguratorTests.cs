using Microsoft.Extensions.DependencyInjection;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Internal.Configurators;

namespace FluentInjections.Tests.Units.Configurator;

public sealed class NetCoreServiceConfiguratorTests
{
    private readonly InternalNetCoreServiceConfiguratorTests _internal = new();

    [Fact]
    void Register_DuplicateRegistrations_UsesLatest()
    {
        _internal.Register_DuplicateRegistrations_UsesLatest();
    }

    [Fact]
    void Register_MergeDescriptors_MergesMetadataAndParameters()
    {
        _internal.Register_MergeDescriptors_MergesMetadataAndParameters();
    }

    [Fact]
    void Register_MultipleImplementations_ResolvesCorrectly()
    {
        _internal.Register_MultipleImplementations_ResolvesCorrectly();
    }

    [Fact]
    void Register_ScopedService_ReturnsDifferentInstancesWithNewScope()
    {
        _internal.Register_ScopedService_ReturnsDifferentInstancesWithNewScope();
    }

    [Fact]
    void Register_ScopedService_ReturnsSameInstanceWithinScope()
    {
        _internal.Register_ScopedService_ReturnsSameInstanceWithinScope();
    }

    [Fact]
    void Register_WithConfigure_CallsConfigure()
    {
        _internal.Register_WithConfigure_CallsConfigure();
    }

    [Fact]
    void Register_WithFactory_RegistersFactory()
    {
        _internal.Register_WithFactory_RegistersFactory();
    }

    [Fact]
    void Register_WithImplementationType_RegistersType()
    {
        _internal.Register_WithImplementationType_RegistersType();
    }

    [Fact]
    void Register_WithInstance_RegistersInstance()
    {
        _internal.Register_WithInstance_RegistersInstance();
    }

    [Fact]
    void Register_WithMetadata_RegistersMetadata()
    {
        _internal.Register_WithMetadata_RegistersMetadata();
    }

    [Fact]
    void Register_WithNameAndFactory_RegistersnameedFactory()
    {
        _internal.Register_WithNameAndFactory_RegistersnameedFactory();
    }

    [Fact]
    void Register_WithName_RegistersnameedTestService()
    {
        _internal.Register_WithName_RegistersnameedTestService();
    }
}

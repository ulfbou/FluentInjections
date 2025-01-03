// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections;
using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Utilities;

namespace FluentInjections.Tests.Units.Configurator;

public class AutofacServiceConfiguratorTests
{
    private readonly InternalAutofacServiceConfiguratorTests _internal = new InternalAutofacServiceConfiguratorTests();

    public AutofacServiceConfiguratorTests()
    {
        _internal.Configurator = new AutofacServiceConfigurator(_internal.Fixture.DependencyBuilder, _internal.Fixture.LoggerMock.Object);
    }

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

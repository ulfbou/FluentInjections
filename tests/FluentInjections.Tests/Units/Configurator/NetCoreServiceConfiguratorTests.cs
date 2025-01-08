// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Internal.Configurators;

namespace FluentInjections.Tests.Units.Configurator;

/// <summary>
/// Represents a test class that contains unit tests for the <see cref="NetCoreServiceConfigurator"/> class.
/// </summary>
public sealed class NetCoreServiceConfiguratorTests
{
    private readonly InternalNetCoreServiceConfiguratorTests _internal = new();
    private readonly ServiceConfiguratorFixture _fixture;
    private readonly ServiceCollection _services;
    private readonly NetCoreServiceConfigurator _configurator;

    public NetCoreServiceConfiguratorTests()
    {
        _fixture = _internal.Fixture;
        _services = _internal.Services;
        _configurator = _internal.Configurator;
    }

    [Fact]
    public void SetConflictResolutionMode_ShouldThrowExceptionForInvalidMode()
    {
        _internal.SetConflictResolutionMode_ShouldThrowExceptionForInvalidMode();
    }

    [Fact]
    void Register_DuplicateRegistrations_UsesLatest()
    {
        _internal.Bind_ServiceType_ToDuplicateRegistrations_UsesLatest();
    }

    [Fact]
    void Register_MergeDescriptors_MergesMetadataAndParameters()
    {
        _internal.Bind_ServiceType_MergeDescriptors_MergesMetadataAndParameters();
    }

    [Fact]
    void Register_MultipleImplementations_ResolvesCorrectly()
    {
        _internal.Bind_ServiceType_ToMultipleNamedImplementations_ResolvesCorrectly();
    }

    [Fact]
    void Register_ScopedService_ReturnsDifferentInstancesWithNewScope()
    {
        _internal.Bind_ServiceType_ScopedService_ReturnsDifferentInstancesWithNewScope();
    }

    [Fact]
    void Register_ScopedService_ReturnsSameInstanceWithinScope()
    {
        _internal.Bind_ServiceType_ScopedService_ReturnsSameInstanceWithinScope();
    }

    [Fact]
    void Register_WithConfigure_CallsConfigure()
    {
        _internal.Bind_ServiceType_WithFactory_CallsConfigure_ResolvesInstanceWithPropertiesCorrectly();
    }

    [Fact]
    void Register_WithFactory_RegistersFactory()
    {
        _internal.Bind_ServiceType_WithFactory_ResolvesInstanceCorrectly();
    }

    [Fact]
    void Register_WithImplementationType_RegistersType()
    {
        _internal.Bind_ServiceType_ToImplementationType_RegistersType();
    }

    [Fact]
    void Register_WithInstance_RegistersInstance()
    {
        _internal.Bind_ServiceType_WithInstance_ResolvesInstanceCorrectly();
    }

    [Fact]
    void Register_WithMetadata_RegistersMetadata()
    {
        _internal.Bind_ServiceType_WithMetadata_ResolvesInstanceAndMetadataCorrectly();
    }

    [Fact]
    void Register_WithNameAndFactory_RegistersnameedFactory()
    {
        _internal.Bind_ServiceType_WithNameAndFactory_ResolvesNamedInstanceCorrectly();
    }

    [Fact]
    void Register_WithName_RegistersnameedTestService()
    {
        _internal.Bind_ServiceType_WithName_ResolvesNamedImplementationCorrectly();
    }

#if OPEN_GENERIC_SUPPORTED
    [Fact]
    void Bind_GenericServiceType_ToGenericType_Should_ResolveCorrectly()
    {
        _internal.Bind_GenericServiceType_ToGenericType_Should_ResolveCorrectly();
    }
#endif

    [Fact]
    void Bind_ServiceType_ToNull_Should_ThrowArgumentNullException()
    {
        _internal.Bind_ServiceType_ToNull_Should_ThrowArgumentNullException();
    }

    [Fact]
    void Prevent_WithDuplicateRegistrations_Should_ThrowInvalidOperationException()
    {
        _internal.Prevent_WithDuplicateRegistrations_Should_ThrowInvalidOperationException();
    }

    [Fact]
    void Bind_ServiceType_WithFactoryNull_Should_ThrowArgumentNullException()
    {
        _internal.Bind_ServiceType_WithFactoryNull_Should_ThrowArgumentNullException();
    }

    [Fact]
    void Bind_ServiceType_WithIgnoreConflictResolution_Should_NotThrowException()
    {
        _internal.Bind_ServiceType_WithIgnoreConflictResolution_Should_NotThrowException();
    }

    [Fact]
    void Bind_ServiceType_WithNameNull_Should_ThrowArgumentNullException()
    {
        _internal.Bind_ServiceType_WithNameNull_Should_ThrowArgumentNullException();
    }

    [Fact]
    void Bind_ServiceType_WithParameterKeyNull_Should_ThrowArgumentNullException()
    {
        _internal.Bind_ServiceType_WithParameterKeyNull_Should_ThrowArgumentNullException();
    }

    [Fact]
    void Bind_ServiceType_WithParameterNull_Should_ThrowArgumentNullException()
    {
        _internal.Bind_ServiceType_WithParameterNull_Should_ThrowArgumentNullException();
    }

    [Fact]
    void Bind_ServiceType_WithPreventConflictResolution_Should_NotThrowException()
    {
        _internal.Bind_ServiceType_WithPreventConflictResolution_Should_NotThrowException();
    }
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Configurators;

/// <summary>
/// Represents a test service configurator that provides methods to configure services within the application.
/// </summary>
internal sealed class TestNetCoreServiceConfigurator : NetCoreServiceConfigurator, IServiceConfigurator, ITestServiceConfigurator
{
    public TestNetCoreServiceConfigurator(IServiceCollection services, ILogger<TestNetCoreServiceConfigurator> logger)
        : base(services, logger)
    { }

    /// <inheritdoc />
    public IEnumerable<ServiceBindingDescriptor> GetDescriptors() => Descriptors;
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Configurators;

internal sealed class TestNetCoreServiceConfigurator : NetCoreServiceConfigurator, ITestServiceConfigurator
{
    public TestNetCoreServiceConfigurator(IServiceCollection services, ILogger<NetCoreServiceConfigurator> logger)
        : base(services, logger)
    { }

    public IEnumerable<ServiceBindingDescriptor> GetDescriptors() => Descriptors;
}

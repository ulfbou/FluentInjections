// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Configurators;

internal sealed class TestNetCoreEndpointConfigurator : EndpointConfigurator, ITestEndpointConfigurator
{
    public TestNetCoreEndpointConfigurator(WebApplication application, ILogger logger) : base(application, logger) { }

    /// <inheritdoc />
    public IReadOnlyList<EndpointDescriptor> GetDescriptors() => _descriptors.AsReadOnly();
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Configurators;

internal sealed class TestNetCoreMiddlewareConfigurator : NetCoreMiddlewareConfigurator<IServiceCollection>
{
    public TestNetCoreMiddlewareConfigurator(IServiceCollection services, ILogger<NetCoreMiddlewareConfigurator<IServiceCollection>> logger) : base(services, logger) { }
    internal void TestValidateBindings() => ValidateBindings();
}

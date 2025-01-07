// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Tests.Internal.Configurators;

internal sealed class TestNetCoreMiddlewareConfigurator : NetCoreMiddlewareConfigurator
{
    public TestNetCoreMiddlewareConfigurator(ApplicationBuilder appBuilder, ILogger<TestNetCoreMiddlewareConfigurator> logger) : base(appBuilder, logger) { }
    internal void TestValidateBindings() => ValidateBindings();
}

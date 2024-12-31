// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Internal.Services;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace FluentInjections.Tests;

public abstract class BaseConfiguratorTest<TConfigurator, TBinding, TFixture> : BaseTest
    where TConfigurator : class, IConfigurator<TBinding>
    where TBinding : IBinding
    where TFixture : class, IConfiguratorFixture<TConfigurator, ServiceCollection>, new()
{
    protected TConfigurator Configurator { get; set; }

    protected BaseConfiguratorTest() : base()
    {
        Configurator = default!;
    }

    /// <inheritdoc />
    protected override void Register()
    {
        Configurator.Register();
        base.Register();
    }
}

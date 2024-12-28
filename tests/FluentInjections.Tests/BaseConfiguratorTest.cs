// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Services;

using System;

namespace FluentInjections.Tests;

public abstract class BaseConfiguratorTest<TConfigurator, TBinding> : BaseTest
    where TConfigurator : IConfigurator<TBinding>
    where TBinding : IBinding
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

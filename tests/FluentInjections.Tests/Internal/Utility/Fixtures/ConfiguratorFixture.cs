// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Utils;

using System.Configuration;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

public abstract class ConfiguratorFixture<TConfigurator, TBuilder>
    where TConfigurator : class, IConfigurator
    where TBuilder : class, new()
{
    public TConfigurator Configurator { get; set; }
    public TBuilder DependencyBuilder { get; set; }

    public ConfiguratorFixture()
    {
        DependencyBuilder = new TBuilder();
        Configurator = Create();
    }

    public virtual void Setup()
    {
        DependencyBuilder = new TBuilder();
        Configurator = Create();
    }

    public virtual void Cleanup() { }

    protected abstract TConfigurator Create();
}

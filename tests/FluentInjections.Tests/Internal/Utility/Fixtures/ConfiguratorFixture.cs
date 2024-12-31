// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

using FluentAssertions.Common;

using FluentInjections;
using FluentInjections.Tests.Internal.Utility.Fixtures;

using System.Configuration;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

public abstract class ConfiguratorFixture<TConfigurator, TService>
    where TConfigurator : class, IConfigurator
    where TService : class, new()
{
    public TConfigurator Configurator { get; set; }
    public TService Services { get; set; }

    public ConfiguratorFixture()
    {
        Services = new TService();
        Configurator = Activator.CreateInstance(typeof(TConfigurator), Services) as TConfigurator
            ?? throw new ConfigurationErrorsException($"Failed to create instance of {typeof(TConfigurator).Name}");
    }

    public virtual void Setup()
    {
        Services = new TService();
        Configurator = Activator.CreateInstance(typeof(TConfigurator), Services) as TConfigurator
            ?? throw new ConfigurationErrorsException($"Failed to create instance of {typeof(TConfigurator).Name}");
    }
    public virtual void Cleanup() { }
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentInjections.Tests.Internal.Utility.Fixtures;
using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Units.Configurator;

internal sealed class InternalNetCoreServiceConfiguratorTests
    : ServiceConfiguratorTests<NetCoreServiceConfigurator, ServiceCollection, NetCoreServiceConfiguratorFixture>
{
    internal override void BuildProvider()
    {
        if (Provider is not null)
        {
            throw new InvalidOperationException("Provider already built");
        }

        Provider = Configurator.BuildServiceProvider();
    }

    protected override IReadOnlyDictionary<string, object> GetMetadata<TService>(string name)
        where TService : class
    {
        return Provider?.GetMetadata<TService>(name) ?? new Dictionary<string, object>();
    }
}

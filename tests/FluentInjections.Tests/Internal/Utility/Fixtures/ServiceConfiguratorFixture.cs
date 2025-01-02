// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Tests.Utility.Fixtures;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

public abstract class ServiceConfiguratorFixture<TConfigurator, TBuilder>
    : ConfiguratorFixture<TConfigurator, TBuilder>, IServiceConfiguratorFixture<TConfigurator, TBuilder>
    where TConfigurator : class, IServiceConfigurator
    where TBuilder : class, new()
{
    public ServiceConfiguratorFixture() : base() { }
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

public class AutofacMiddlewareConfiguratorFixture : MiddlewareConfiguratorFixture<AutofacMiddlewareConfigurator, ContainerBuilder>
{
    public IContainer Container { get; private set; }
    public ILifetimeScope Scope { get; private set; }
    public AutofacServiceProvider Provider { get; private set; }

    public AutofacMiddlewareConfiguratorFixture() : base()
    {
        Container = Services.Build();
        Scope = Container.BeginLifetimeScope();
        Provider = new AutofacServiceProvider(Scope);
    }
}

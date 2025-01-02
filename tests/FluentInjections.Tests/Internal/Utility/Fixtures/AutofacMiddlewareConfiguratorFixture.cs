// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

using Moq;

using System.Reflection.PortableExecutable;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

internal class AutofacMiddlewareConfiguratorFixture : MiddlewareConfiguratorFixture<AutofacMiddlewareConfigurator, ContainerBuilder>
{
    public IContainer Container { get; private set; }
    public ILifetimeScope Scope { get; private set; }
    public AutofacServiceProvider Provider { get; private set; }

    public AutofacMiddlewareConfiguratorFixture() : base()
    {
        Container = DependencyBuilder.Build();
        Scope = Container.BeginLifetimeScope();
        Provider = new AutofacServiceProvider(Scope);
        Configurator = Create(); // Required since Configurator's call to Create returns a default value
    }

    protected override AutofacMiddlewareConfigurator Create()
    {
        return Container is null ? default! : new(Container, MockLogger.Object);
    }
}

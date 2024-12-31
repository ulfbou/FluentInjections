// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

public class AutofacServiceConfiguratorFixture : ServiceConfiguratorFixture<AutofacServiceConfigurator, ContainerBuilder>
{
    public IContainer Container { get; private set; }

    public AutofacServiceConfiguratorFixture() : base()
    {
        Container = Services.Build();
    }
}

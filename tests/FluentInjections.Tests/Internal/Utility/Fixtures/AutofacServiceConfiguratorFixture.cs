// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Utils;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

internal class AutofacServiceConfiguratorFixture : ServiceConfiguratorFixture<AutofacServiceConfigurator, ContainerBuilder>
{
    public IContainer Container { get; private set; }

    public AutofacServiceConfiguratorFixture() : base()
    {
        Container = base.DependencyBuilder.Build();
    }

    protected override AutofacServiceConfigurator Create()
    {
        var logger = LoggerUtility.CreateLogger<AutofacServiceConfigurator>();
        return new(DependencyBuilder, logger);
    }
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Tests.Services;

namespace FluentInjections.Tests.Modules.Unnamed;

public sealed class UnnamedTestServiceModule : Module<IServiceConfigurator>, IServiceModule
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Bind<ITestService>()
                    .To<TestServiceWithDefaultValues>()
                    .AsSingleton();
    }
}

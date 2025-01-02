// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Utility.Fixtures;

using Microsoft.Extensions.Logging;

using Moq;

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

public abstract class MiddlewareConfiguratorFixture<TConfigurator, TService>
    : ConfiguratorFixture<TConfigurator, TService>, IMiddlewareConfiguratorFixture<TConfigurator, TService>, IConfiguratorFixture<TConfigurator, TService>
    where TConfigurator : class, IMiddlewareConfigurator
    where TService : class, new()
{
    // Mock logger
    internal Mock<ILogger<AutofacMiddlewareConfigurator>> MockLogger { get; private set; }

    public MiddlewareConfiguratorFixture() : base()
    {
        MockLogger = new Mock<ILogger<AutofacMiddlewareConfigurator>>();
    }

    public override void Setup()
    {
        base.Setup();
        MockLogger = new Mock<ILogger<AutofacMiddlewareConfigurator>>();
    }
}

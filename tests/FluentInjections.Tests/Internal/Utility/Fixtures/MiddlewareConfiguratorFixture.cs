// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Tests.Internal.Utility.Fixtures;

public abstract class MiddlewareConfiguratorFixture<TConfigurator, TService> : ConfiguratorFixture<TConfigurator, TService>
    where TConfigurator : class, IMiddlewareConfigurator
    where TService : class, new()
{
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Tests.Utility.Fixtures;

public interface IMiddlewareConfiguratorFixture<TConfigurator, TService, TProvider> : IConfiguratorFixture<TConfigurator, TService>
    where TConfigurator : class, IMiddlewareConfigurator
    where TService : class
{
    TProvider Provider { get; set; }
}

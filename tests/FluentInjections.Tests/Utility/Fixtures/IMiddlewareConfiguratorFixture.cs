// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Utility.Fixtures;

public interface IMiddlewareConfiguratorFixture : IConfiguratorFixture<IServiceConfigurator, ServiceCollection, NetCoreServiceProvider> { }

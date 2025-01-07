// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Tests.Internal.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Utility.Fixtures;

public interface IServiceConfiguratorFixture<TConfigurator, TServices, TProvider> : IConfiguratorFixture<TConfigurator, TServices, TProvider>
    where TConfigurator : class, IServiceConfigurator
    where TServices : class, IServiceCollection, new()
    where TProvider : class, IServiceProvider
{ }

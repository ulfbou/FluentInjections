// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Tests.Internal.Utility.Fixtures;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
namespace FluentInjections.Tests.Utility.Fixtures;

public interface IServiceConfiguratorFixture<TConfigurator, TService> : IConfiguratorFixture<TConfigurator, TService>
    where TConfigurator : class, IServiceConfigurator
    where TService : class
{
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Utility.Fixtures;

public interface IConfiguratorFixture<TConfigurator, TContainer>
    where TConfigurator : class, IConfigurator
    where TContainer : class
{
    TConfigurator Configurator { get; set; }
    TContainer DependencyBuilder { get; set; }

    void Setup();
    void Cleanup();
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

public interface IConfigurableModule : IModule
{
    int Priority { get; }
    void Configure(IServiceConfigurator configurator);
    void Configure(IMiddlewareConfigurator configurator);
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

public abstract partial class Module : IConfigurableModule
{
    public virtual void Configure(IServiceConfigurator configurator) { }
    public virtual void Configure(IMiddlewareConfigurator configurator) { }
}

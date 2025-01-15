// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

public abstract partial class Module :
    IConfigurableModule,
    IConfigurableModule<IServiceConfigurator>,
    IConfigurableModule<IMiddlewareConfigurator>
{
    public bool CanHandle<T>() where T : IConfigurator => throw new NotImplementedException();
    public bool CanHandle(Type configuratorType) => throw new NotImplementedException();

    public virtual void Configure(IServiceConfigurator configurator) { }
    public virtual void Configure(IMiddlewareConfigurator configurator) { }
}

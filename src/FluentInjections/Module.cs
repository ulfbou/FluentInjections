// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Validation;

namespace FluentInjections;

public abstract class Module<TConfigurator> : IConfigurableModule<TConfigurator> where TConfigurator : IConfigurator
{
    public Type ConfiguratorType { get; set; }

    public Module()
    {
        ConfiguratorType = typeof(TConfigurator);
    }

    /// <inheritdoc />
    public virtual bool CanHandle<T>() where T : IConfigurator => ConfiguratorType.IsAssignableFrom(typeof(T));

    /// <inheritdoc />
    public virtual bool CanHandle(Type configuratorType)
    {
        Guard.NotNull(configuratorType, nameof(configuratorType));
        return ConfiguratorType.IsAssignableFrom(configuratorType);
    }

    public abstract void Configure(TConfigurator configurator);
}

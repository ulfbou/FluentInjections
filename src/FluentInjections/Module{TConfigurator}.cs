// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Wrappers;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;

namespace FluentInjections;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TConfigurator"></typeparam>
public abstract class Module<TConfigurator> : IConfigurableModule<TConfigurator> where TConfigurator : IConfigurator
{
    public Type ConfiguratorType { get; set; }
    public ApplicationBuilderWrapper Application { get; private set; }
    public int Priority { get; set; }

    public Module(IApplicationBuilder application)
    {
        ConfiguratorType = typeof(TConfigurator);
        Application = application as ApplicationBuilderWrapper ?? throw new ArgumentNullException(nameof(application));
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

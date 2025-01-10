// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Utils;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace FluentInjections;

/// <summary>
/// Represents a module that provides methods to configure middleware with .Net Core. 
/// </summary>
internal sealed class FluentInjectionsNetCoreServiceModule : FluentInjectionsModule
{
    private readonly IServiceCollection _services;

    public FluentInjectionsNetCoreServiceModule(IServiceCollection services, Assembly[] assemblies) : base(assemblies)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    internal void Load()
    {
        var serviceConfigurator = DependencyInjection.ServiceConfigurator;

        foreach (var assembly in _assemblies)
        {
            RegisterModules<IServiceConfigurator, IServiceModule>(assembly, serviceConfigurator);
        }

        serviceConfigurator.Register();
    }
}

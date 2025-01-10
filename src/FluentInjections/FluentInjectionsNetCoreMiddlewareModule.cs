// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;

using System.Reflection;

namespace FluentInjections;

/// <summary>
/// Represents a module that provides methods to configure middleware with .Net Core. 
/// </summary>
internal sealed class FluentInjectionsNetCoreMiddlewareModule : FluentInjectionsModule
{
    private readonly IApplicationBuilder _appBuilder;

    public FluentInjectionsNetCoreMiddlewareModule(IApplicationBuilder appBuilder, Assembly[] assemblies) : base(assemblies)
    {
        _appBuilder = appBuilder ?? throw new ArgumentNullException(nameof(appBuilder));
    }

    internal void Load()
    {
        var middlewareConfigurator = DependencyInjection.MiddlewareConfigurator;

        foreach (var assembly in _assemblies)
        {
            RegisterModules<IMiddlewareConfigurator, IMiddlewareModule>(assembly, middlewareConfigurator);
        }
        middlewareConfigurator.Register();
    }
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;

using FluentInjections;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Utils;

using System.Reflection;

namespace FluentInjections;

/// <summary>
/// Represents a module that provides methods to configure services and middleware within the application using Autofac.
/// </summary>
internal sealed class FluentInjectionsAutofacModule : FluentInjectionsModule
{
    private readonly IContainer _container;

    public FluentInjectionsAutofacModule(IContainer container, Assembly[] assemblies) : base(assemblies)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    protected override void Load(ContainerBuilder builder)
    {
        var serviceConfigurator = new AutofacServiceConfigurator(builder, LoggerUtility.CreateLogger<AutofacServiceConfigurator>());
        var middlewareConfigurator = new AutofacMiddlewareConfigurator(_container, LoggerUtility.CreateLogger<AutofacMiddlewareConfigurator>());

        foreach (var assembly in _assemblies)
        {
            RegisterModulesFromAssembly(assembly, serviceConfigurator, middlewareConfigurator);
        }

        serviceConfigurator.Register();
        middlewareConfigurator.Register();
    }
}

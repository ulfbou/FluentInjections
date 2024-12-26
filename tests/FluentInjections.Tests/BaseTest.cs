using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Services;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Tests;

public abstract class BaseTest
{
    protected IServiceCollection Services { get; }
    protected ContainerBuilder Builder { get; }
    internal ServiceConfigurator Configurator { get; }
    protected IContainer Container { get; private set; }

    protected BaseTest()
    {
        Services = new ServiceCollection();
        Builder = new ContainerBuilder();
        Configurator = new ServiceConfigurator(Builder);
        Container = default!;
    }

    protected void Register()
    {
        Configurator.Register();
        Builder.Populate(Services);
        Container = Builder.Build();
    }

    protected T? Resolve<T>() where T : notnull
        => Container.Resolve<T>();

    protected T? Resolve<T>(string name) where T : notnull
        => Container.ResolveNamed<T>(name);

    protected T? Resolve<T>(params Parameter[] parameters) where T : notnull
        => Container.Resolve<T>(parameters);

    protected T? Resolve<T>(string name, params Parameter[] parameters) where T : notnull
        => Container.ResolveNamed<T>(name, parameters);

    protected ILifetimeScope CreateScope()
        => Container.BeginLifetimeScope();

    protected void RegisterService<TService>(Action<IServiceBinding<TService>> configure) where TService : class
    {
        var binding = Configurator.Bind<TService>();
        configure(binding);
    }
}

using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Services;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace FluentInjections.Tests;

public abstract class BaseConfiguratorTest<TConfigurator, TBinding>
    where TConfigurator : IConfigurator<TBinding>
    where TBinding : IBinding
{
    protected IServiceCollection Services { get; }
    protected ContainerBuilder Builder { get; }
    internal abstract TConfigurator Configurator { get; }
    protected IContainer Container { get; private set; }
    private ILifetimeScope? _scope;

    protected BaseConfiguratorTest()
    {
        Services = new ServiceCollection();
        Builder = new ContainerBuilder();
        Builder.Populate(Services);
        Container = default!;
    }

    protected void Register()
    {
        Configurator.Register();

        if (Container is not null)
        {
            throw new InvalidOperationException("Container has already been built.");
        }

        Container = Builder.Build();
    }

    protected T Resolve<T>() where T : notnull
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container has not been built.");
        }

        return Container.Resolve<T>();
    }

    protected T Resolve<T>(string name) where T : notnull
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container has not been built.");
        }

        return Container.ResolveNamed<T>(name);
    }

    protected ILifetimeScope CreateScope()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container has not been built.");
        }

        if (_scope is not null)
        {
            _scope.Dispose();
        }

        _scope = Container.BeginLifetimeScope();
        return _scope;
    }

    protected void DisposeScope()
    {
        _scope?.Dispose();
        _scope = null;
    }
}

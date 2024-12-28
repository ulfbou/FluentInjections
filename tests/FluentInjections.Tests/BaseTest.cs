// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests;

public abstract class BaseTest
{
    protected IServiceCollection Services { get; }
    protected ContainerBuilder Builder { get; }
    protected IContainer Container { get; set; }
    protected IServiceProvider ServiceProvider { get; set; }
    protected ILifetimeScope? Scope { get; set; }

    protected BaseTest()
    {
        Services = new ServiceCollection();
        Builder = new ContainerBuilder();
        Container = default!;
        ServiceProvider = default!;
        Scope = default!;
    }

    /// <summary>
    /// Registers the services with the container.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the container has already been built.</exception>
    protected virtual void Register()
    {
        if (Container is not null)
        {
            throw new InvalidOperationException("Container has already been built.");
        }

        Builder.Populate(Services);
        Container = Builder.Build();
        ServiceProvider = new AutofacServiceProvider(Container);
    }

    /// <summary>
    /// Resolves a service from the container.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <returns>The resolved service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the container has not been built.</exception>
    protected TService Resolve<TService>() where TService : notnull
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container has not been built. Ensure that Register is called prior to calling Resolve<T>.");
        }

        return Container.Resolve<TService>();
    }

    /// <summary>
    /// Resolves a named service from the container.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <param name="name">The name of the service to resolve.</param>
    /// <returns>The resolved service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the container has not been built.</exception>
    protected TService Resolve<TService>(string name) where TService : notnull
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container has not been built. Ensure that Register is called prior to calling Resolve<T>.");
        }

        return Container.ResolveNamed<TService>(name);
    }

    /// <summary>
    /// Creates a new lifetime scope.
    /// </summary>
    /// <returns>The new lifetime scope.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the container has not been built.</exception>
    protected ILifetimeScope CreateScope()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("Container has not been built. Ensure that Register is called prior to calling Resolve<T>.");
        }

        DisposeScope();
        Scope = Container.BeginLifetimeScope();
        return Scope;
    }

    /// <summary>
    /// Disposes the current lifetime scope.
    /// </summary>
    protected void DisposeScope()
    {
        Scope?.Dispose();
        Scope = null;
    }
}

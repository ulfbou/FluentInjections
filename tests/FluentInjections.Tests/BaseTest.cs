using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FluentInjections.Tests
{
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
            Container = default!;
        }

        protected void Register()
        {
            Configurator.Register();
            Builder.Populate(Services);
            Container = Builder.Build();
        }

        protected T Resolve<T>() where T : notnull
            => Container.Resolve<T>();

        protected T Resolve<T>(string name) where T : notnull
            => Container.ResolveNamed<T>(name);

        protected T Resolve<T>(params Parameter[] parameters) where T : notnull
            => Container.Resolve<T>(parameters);

        protected T Resolve<T>(string name, params Parameter[] parameters) where T : notnull
            => Container.ResolveNamed<T>(name, parameters);

        protected ILifetimeScope CreateScope()
        {
            if (_scope != null && _scope.Disposer.IsDisposed)
            {
                _scope.Dispose();
            }
            _scope = Container.BeginLifetimeScope();
            return _scope;
        }

        protected void Register<TService>(Action<IBinding<TService>> configure) where TService : class
        {
            var binding = Configurator.Bind<TService>();
            configure(binding);
        }

        protected void DisposeScope()
        {
            _scope?.Dispose();
            _scope = null;
        }

        protected virtual void RegisterAdditionalServices()
        {
            // Override this method in derived classes to register additional services if needed
        }

        protected virtual void InitializeConfigurator()
        {
            // Override this method in derived classes to initialize the configurator if needed
        }
    }
}
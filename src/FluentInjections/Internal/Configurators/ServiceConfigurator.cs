using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Extensions;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;

namespace FluentInjections.Internal.Configurators;

public class ServiceConfigurator : IServiceConfigurator, IConfigurator<IServiceBinding>
{
    private readonly IServiceCollection _services;
    private readonly ContainerBuilder _builder;
    private readonly List<IServiceBinding> _bindings = new();
    private readonly IComponentRegistryBuilder? _componentRegistry;

    public ServiceConfigurator(IServiceCollection services, IComponentRegistryBuilder? componentRegistry = null)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _builder = new();
        _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
    }

    public IServiceBinding<TService> Bind<TService>() where TService : class, new()
    {
        var binding = new ServiceBinding<TService>(_builder);
        _bindings.Add(binding);

        return binding;
    }

    public void Unbind<TService>()
    {
        var binding = _bindings.FirstOrDefault(binding => binding.Descriptor.BindingType == typeof(TService));

        if (binding is null)
        {
            throw new InvalidOperationException($"Service of type {typeof(TService).Name} not found.");
        }

        _bindings.Remove(binding);

        // TODO: Remove the binding from the container builder.
    }

    public void Register()
    {
        foreach (var binding in _bindings)
        {
            binding.Register();
        }
    }

    internal class ServiceBinding<TService> : IServiceBinding<TService> where TService : class, new()
    {
        public ServiceBindingDescriptor Descriptor { get; }
        private ContainerBuilder _builder;

        public ServiceBinding(ContainerBuilder builder)
        {
            Descriptor = new ServiceBindingDescriptor(typeof(TService));
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public IServiceBinding<TService> AsScoped()
        {
            Descriptor.Lifetime = ServiceLifetime.Scoped;
            return this;
        }

        public IServiceBinding<TService> AsSelf()
        {
            Descriptor.ImplementationType = typeof(TService);
            return this;
        }

        public IServiceBinding<TService> AsSingleton()
        {
            Descriptor.Lifetime = ServiceLifetime.Singleton;
            return this;
        }

        public IServiceBinding<TService> AsTransient()
        {
            Descriptor.Lifetime = ServiceLifetime.Transient;
            return this;
        }

        public IServiceBinding<TService> To<TImplementation>() where TImplementation : class, TService
        {
            Descriptor.ImplementationType = typeof(TImplementation);
            return this;
        }

        public IServiceBinding<TService> To(Type implementationType)
        {
            ArgumentGuard.NotNull(implementationType, nameof(implementationType));

            Descriptor.ImplementationType = implementationType;
            return this;
        }

        public IServiceBinding<TService> WithFactory(Func<IServiceProvider, TService> factory)
        {
            ArgumentGuard.NotNull(factory, nameof(factory));

            Descriptor.Factory = factory;
            return this;
        }

        public IServiceBinding<TService> WithInstance(TService instance)
        {
            ArgumentGuard.NotNull(instance, nameof(instance));

            Descriptor.Instance = instance;
            return this;
        }

        public IServiceBinding<TService> WithLifetime(ServiceLifetime lifetime)
        {
            Descriptor.Lifetime = lifetime;
            return this;
        }

        public IServiceBinding<TService> WithName(string name)
        {
            Descriptor.Name = name;
            return this;
        }

        public IServiceBinding<TService> WithParameters(object parameters)
        {
            ArgumentGuard.NotNull(parameters, nameof(parameters));

            if (Descriptor.Instance is not null)
            {
                throw new InvalidOperationException("Cannot specify parameters for an instance.");
            }

            if (parameters is IReadOnlyDictionary<string, object> dictionary)
            {
                Descriptor.Parameters = dictionary;
            }
            else
            {
                throw new ArgumentException("Parameters must be a dictionary.");
            }

            return this;
        }

        public IServiceBinding<TService> WithParameters(IReadOnlyDictionary<string, object> parameters)
        {
            Descriptor.Parameters = parameters;
            return this;
        }

        public IServiceBinding<TService> Configure(Action<TService> configure)
        {
            Descriptor.Configure = service => configure((TService)service);
            return this;
        }

        public IServiceBinding<TService> Configure<TOptions>(Action<TOptions> configure) where TOptions : class
        {
            Descriptor.ConfigureOptions = options => configure((TOptions)options);
            return this;
        }

        public void Register()
        {
            if (Descriptor.ImplementationType is not null)
            {
                var builder = RegisterBuilder(_builder.RegisterType(Descriptor.ImplementationType!).As<TService>());
                var parameterBuilder = RegisterParameters(builder.AsReflectionActivator());
                parameterBuilder = RegisterConfiguration(parameterBuilder);
            }
            else if (Descriptor.Instance is not null)
            {
                var builder = RegisterBuilder(_builder.RegisterInstance(Descriptor.Instance!).As<TService>());
                var parameterBuilder = RegisterParameters(builder.AsReflectionActivator());
                parameterBuilder = RegisterConfiguration(parameterBuilder);
            }
            else if (Descriptor.Factory is not null)
            {
                var builder = RegisterBuilder(_builder.Register(c => Descriptor.Factory!(c.Resolve<IServiceProvider>())).As<TService>());
                var parameterBuilder = RegisterParameters(builder.AsReflectionActivator());
                parameterBuilder = RegisterConfiguration(parameterBuilder);
            }
            else
            {
                var builder = RegisterBuilder(_builder.RegisterType<TService>().AsSelf());
                var parameterBuilder = RegisterParameters(builder.AsReflectionActivator());
                parameterBuilder = RegisterConfiguration(parameterBuilder);
            }
        }

        private IRegistrationBuilder<TLimit, TActivatorData, TStyle> RegisterBuilder<TLimit, TActivatorData, TStyle>(IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder)
        {
            if (Descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                builder = builder.SingleInstance();
            }
            else if (Descriptor.Lifetime == ServiceLifetime.Scoped)
            {
                builder = builder.InstancePerLifetimeScope();
            }
            else
            {
                builder = builder.InstancePerDependency();
            }

            if (string.IsNullOrEmpty(Descriptor.Name))
            {
                builder = builder.Named<TService>(Descriptor.Name!);
            }

            return builder;
        }

        private IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle> RegisterParameters<TLimit, TStyle>(IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle> builder)
        {
            // Add parameters if provided
            if (Descriptor.Parameters is IReadOnlyDictionary<string, object> dictionary)
            {
                var parameters = dictionary.Select(parameter => new ResolvedParameter((pi, ctx) => pi.Name == parameter.Key, (pi, ctx) => parameter.Value)).ToList();

                return builder.WithParameters(parameters);
            }

            throw new InvalidOperationException("Parameters must be a dictionary.");
        }

        private IRegistrationBuilder<TLimit, TActivatorData, TStyle> RegisterConfiguration<TLimit, TActivatorData, TStyle>(IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder) where TActivatorData : ReflectionActivatorData
        {
            if (Descriptor.Configure is not null)
            {
                if (Descriptor.Instance is null)
                {
                    throw new InvalidOperationException("Cannot configure an instance.");
                }

                builder.OnActivating(e => Descriptor.Configure(e.Instance!));
            }
            if (Descriptor.ConfigureOptions is not null)
            {
                if (Descriptor.Instance is null)
                {
                    throw new InvalidOperationException("Cannot configure an instance.");
                }

                builder.OnActivating(e => Descriptor.ConfigureOptions(e.Instance!));
            }

            return builder;
        }
    }
}

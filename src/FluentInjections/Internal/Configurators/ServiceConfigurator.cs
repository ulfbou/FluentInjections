using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Extensions.DependencyInjection;

using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Extensions;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

namespace FluentInjections.Internal.Configurators;

internal class ServiceConfigurator : IServiceConfigurator
{
    private readonly ContainerBuilder _builder;
    private readonly List<ServiceBindingDescriptor> _bindings = new();

    internal ContainerBuilder Builder => _builder;
    internal IReadOnlyList<ServiceBindingDescriptor> Bindings => _bindings.AsReadOnly();

    public ServiceConfigurator(ContainerBuilder builder)
    {
        _builder = builder;
    }

    public IServiceBinding<TService> Bind<TService>() where TService : notnull
    {
        var descriptor = new ServiceBindingDescriptor(typeof(TService));

        _bindings.Add(descriptor);
        return new ServiceBinding<TService>(this, descriptor);
    }

    /// <summary>
    /// Gets the service binding for the specified type.
    /// </summary>
    /// <param name="type">The type of the service.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not found.</exception>
    /// <remarks>
    /// This method is used internally to get the service binding for a specific type.
    /// </remarks>
    internal ServiceBindingDescriptor GetBinding(Type type)
    {
        ServiceBindingDescriptor? descriptor = _bindings.FirstOrDefault(descriptor => descriptor.BindingType == type);

        if (descriptor is null)
        {
            throw new InvalidOperationException($"Service of type {type.Name} not found.");
        }

        return descriptor;
    }

    public void Register()
    {
        foreach (var binding in _bindings)
        {
            Register(binding);
        }
    }

    private void Register(ServiceBindingDescriptor descriptor)
    {

        if (descriptor.ImplementationType is not null)
        {
            var registration = RegisterBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(
                _builder.RegisterType(descriptor.ImplementationType).As(descriptor.BindingType),
                descriptor);
        }
        else if (descriptor.Instance is not null)
        {
            RegisterBuilder<object, SimpleActivatorData, SingleRegistrationStyle>(
                _builder.RegisterInstance(descriptor.Instance!).As(descriptor.BindingType),
                descriptor);
        }
        else if (descriptor.Factory is not null)
        {
            RegisterBuilder<object, SimpleActivatorData, SingleRegistrationStyle>(
                _builder.Register(c => descriptor.Factory!(c.Resolve<IServiceProvider>())).As(descriptor.BindingType),
                descriptor);
        }
        else
        {
            RegisterBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(
                _builder.RegisterType(descriptor.BindingType).AsSelf(),
                descriptor);
        }
    }

    private IRegistrationBuilder<TLimit, TActivatorData, TStyle> RegisterBuilder<TLimit, TActivatorData, TStyle>(
        IRegistrationBuilder<TLimit, TActivatorData, TStyle> builder, ServiceBindingDescriptor descriptor)
    {
        // Handle SimpleActivatorData-based registrations here
        if (descriptor.Lifetime == ServiceLifetime.Singleton)
        {
            builder = builder.SingleInstance();
        }
        else if (descriptor.Lifetime == ServiceLifetime.Scoped)
        {
            builder = builder.InstancePerLifetimeScope();
        }
        else
        {
            builder = builder.InstancePerDependency();
        }

        if (!string.IsNullOrEmpty(descriptor.Name))
        {
            builder = builder.Named(descriptor.Name!, descriptor.BindingType);
        }

        if (descriptor.Configure is not null)
        {
            if (descriptor.Instance is null)
            {
                throw new InvalidOperationException("Cannot configure an instance.");
            }

            builder = builder.OnActivating(e =>
            {
                descriptor.Configure(e.Instance!);
            });
        }

        if (descriptor.Parameters is not null)
        {
            if (builder.IsReflectionData())
            {
                RegisterParameters(builder.AsReflectionActivator(), descriptor);
            }
            else
            {
                throw new InvalidOperationException($"The builder does not contain reflection data.");
            }
        }

        return builder;
    }

    private void RegisterParameters<TLimit, TStyle>(IRegistrationBuilder<TLimit, ReflectionActivatorData, TStyle> builder, ServiceBindingDescriptor descriptor)
    {
        if (descriptor.Parameters is not IReadOnlyDictionary<string, object> dictionary)
        {
            throw new InvalidOperationException("Parameters must be a dictionary.");
        }

        var parameters = dictionary.Select(parameter => new ResolvedParameter((pi, ctx) => pi.Name == parameter.Key, (pi, ctx) => parameter.Value)).ToList();
        builder.WithParameters(parameters);
    }

    internal class ServiceBinding<TService> : IServiceBinding<TService> where TService : notnull
    {
        private readonly ServiceConfigurator _configurator;
        private readonly ServiceBindingDescriptor _descriptor;

        public ServiceBindingDescriptor Descriptor => _descriptor;
        internal ServiceConfigurator Configurator => _configurator;

        public ServiceBinding(ServiceConfigurator configurator, ServiceBindingDescriptor descriptor)
        {
            _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public IServiceBinding<TService> To<TImplementation>() where TImplementation : class, TService => To(typeof(TImplementation));

        public IServiceBinding<TService> To(Type implementationType)
        {
            ArgumentGuard.NotNull(implementationType, nameof(implementationType));

            Descriptor.ImplementationType = implementationType;
            return this;
        }

        public IServiceBinding<TService> AsSelf()
        {
            Descriptor.ImplementationType = Descriptor.BindingType;
            return this;
        }

        public IServiceBinding<TService> WithInstance(TService instance)
        {
            ArgumentGuard.NotNull(instance, nameof(instance));

            if (Descriptor.ImplementationType is not null)
            {
                Debug.WriteLine("Warning: Implementation type is already set. Setting instance will override the implementation type.");
                Descriptor.ImplementationType = default;
            }

            if (Descriptor.Factory is not null)
            {
                Debug.WriteLine("Warning: Factory is already set. Setting instance will override the factory.");
                Descriptor.Factory = default;
            }

            Descriptor.Instance = instance;
            return this;
        }

        public IServiceBinding<TService> WithFactory(Func<IServiceProvider, TService> factory)
        {
            ArgumentGuard.NotNull(factory, nameof(factory));

            if (Descriptor.ImplementationType is not null)
            {
                Debug.WriteLine("Warning: Implementation type is already set. Setting factory will override the implementation type.");
                Descriptor.ImplementationType = default;
            }

            if (Descriptor.Instance is not null)
            {
                Debug.WriteLine("Warning: Instance is already set. Setting factory will override the instance.");
                Descriptor.Instance = default;
            }

            Descriptor.Factory = sp => factory(sp);
            return this;
        }

        public IServiceBinding<TService> WithName(string name)
        {
            ArgumentGuard.NotNullOrEmpty(name, nameof(name));

            Descriptor.Name = name;
            return this;
        }

        public IServiceBinding<TService> WithLifetime(ServiceLifetime lifetime)
        {
            Descriptor.Lifetime = lifetime;
            return this;
        }

        public IServiceBinding<TService> AsSingleton()
        {
            Descriptor.Lifetime = ServiceLifetime.Singleton;
            return this;
        }

        public IServiceBinding<TService> AsScoped()
        {
            Descriptor.Lifetime = ServiceLifetime.Scoped;
            return this;
        }

        public IServiceBinding<TService> AsTransient()
        {
            Descriptor.Lifetime = ServiceLifetime.Transient;
            return this;
        }

        public IServiceBinding<TService> WithParameter(string name, object value)
        {
            ArgumentGuard.NotNullOrEmpty(name, nameof(name));
            ArgumentGuard.NotNull(value, nameof(value));

            if (Descriptor.Instance is not null)
            {
                throw new InvalidOperationException("Cannot specify parameters for an instance.");
            }

            if (Descriptor.Parameters.ContainsKey(name))
            {
                throw new InvalidOperationException($"Parameter with name {name} already exists.");
            }

            Descriptor.Parameters.Add(name, value);
            return this;
        }

        public IServiceBinding<TService> WithParameters(object parameters)
        {
            if (parameters is not IReadOnlyDictionary<string, object> dictionary)
            {
                // convert object into dictionary
                var properties = parameters?.GetType().GetProperties();
                dictionary = properties?.ToDictionary(property => property.Name, property => property.GetValue(parameters)).AsReadOnly()!;
            }

            return WithParameters(dictionary);
        }

        public IServiceBinding<TService> WithParameters(IReadOnlyDictionary<string, object> parameters)
        {
            ArgumentGuard.NotNull(parameters, nameof(parameters));

            foreach (var parameter in parameters)
            {
                if (Descriptor.Parameters.ContainsKey(parameter.Key))
                {
                    throw new InvalidOperationException($"Parameter with name {parameter.Key} already exists.");
                }

                Descriptor.Parameters.Add(parameter.Key, parameter.Value);
            }

            return this;
        }

        public IServiceBinding<TService> Configure(Action<TService> configure)
        {
            Descriptor.Configure = service => configure((TService)service);
            return this;
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

            if (!string.IsNullOrEmpty(Descriptor.Name))
            {
                builder = builder.Named<TService>(Descriptor.Name!);
            }

            return builder;
        }
    }
}

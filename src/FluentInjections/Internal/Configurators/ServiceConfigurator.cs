// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Extensions;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Utils;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Diagnostics;
using System.Text;

using ServiceDescriptor = FluentInjections.Internal.Descriptors.ServiceDescriptor;
using DotNetServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using FluentInjections.Internal.Managers;

namespace FluentInjections.Internal.Configurators;

internal abstract class ServiceConfigurator : BaseConfigurator<IServiceBinding, ServiceDescriptor>, IServiceConfigurator
{
    protected readonly ServiceManager _serviceManager = ServiceManager.Instance;

    public virtual IServiceCollection Services { get; }

    internal ServiceConfigurator(ILogger logger) : base(logger)
    {
        Services = new ServiceCollection();
    }

    internal ServiceConfigurator(IServiceCollection services, ILogger logger) : base(logger)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    // TODO: Handle open generic types
    /// <inheritdoc />
    public IServiceBuilder<TService> Bind<TService>() where TService : notnull
    {
        var descriptor = new ServiceDescriptor(typeof(TService), this);
        var existingDescriptor = _descriptors.FirstOrDefault(binding => binding.BindingType == descriptor.BindingType && binding.Name == descriptor.Name);

        _descriptors.Add(descriptor);
        return new ServiceBuilder<TService>(this, descriptor);
    }

    /// <inheritdoc />
    public IServiceBuilder Bind(Type serviceType)
    {
        Guard.NotNull(serviceType, nameof(serviceType));

        if (!serviceType.IsInterface && serviceType.IsAbstract)
        {
            throw new InvalidOperationException("Cannot bind abstract types to themselves.");
        }

        var descriptor = new ServiceDescriptor(serviceType, this);
        var existingDescriptor = _descriptors.FirstOrDefault(binding => binding.BindingType == descriptor.BindingType && binding.Name == descriptor.Name);
        _descriptors.Add(descriptor);
        return new ServiceBindingBuilder(this, descriptor);
    }

    #region Validate Bindings
    protected internal override void ValidateBindings()
    {
        var duplicateGroups = _descriptors.GroupBy(binding => new { binding.BindingType, binding.Name })
                                          .Where(group => group.Count() > 1)
                                          .ToList();

        if (duplicateGroups.Any())
        {
            StringBuilder sb = new();
            sb.AppendLine("Duplicate service bindings found:");

            foreach (var group in duplicateGroups)
            {
                var key = group.Key;
                var message = $"Duplicate service binding for type {key.BindingType.Name}";

                if (key.Name is not null)
                {
                    message += $" with name {key.Name}";
                }

                switch (ConflictResolution)
                {
                    case ConflictResolutionMode.WarnAndReplace:
                        _logger.LogWarning(message);
                        ReplaceBindings(group);
                        break;
                    case ConflictResolutionMode.Replace:
                        ReplaceBindings(group);
                        break;
                    case ConflictResolutionMode.Prevent:
                        sb.AppendLine(message);
                        break;
                    case ConflictResolutionMode.Merge:
                        _logger.LogWarning(message);
                        MergeBindings(group);
                        break;
                    case ConflictResolutionMode.Ignore:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid conflict resolution mode.");
                }
            }

            if (ConflictResolution == ConflictResolutionMode.Prevent && sb.Length > 0)
            {
                throw new InvalidOperationException(sb.ToString());
            }
        }
    }

    private void ReplaceBindings(IGrouping<object, ServiceDescriptor> group)
    {
        Guard.NotNull(group, nameof(group));

        var bindings = group.ToList();

        // Keep the last binding and remove the rest
        var lastBinding = bindings.Last();
        _descriptors.RemoveAll(binding => binding.BindingType == lastBinding.BindingType && binding.Name == lastBinding.Name && binding != lastBinding);
    }

    private void MergeBindings(IGrouping<object, ServiceDescriptor> group)
    {
        Guard.NotNull(group, nameof(group));

        var bindings = group.ToList();
        var primaryBinding = bindings.First();

        foreach (var binding in bindings.Skip(1))
        {
            MergeDescriptors(primaryBinding, binding);
            _descriptors.Remove(binding);
        }
    }

    private void MergeDescriptors(ServiceDescriptor existingDescriptor, ServiceDescriptor newDescriptor)
    {
        Guard.NotNull(existingDescriptor, nameof(existingDescriptor));
        Guard.NotNull(newDescriptor, nameof(newDescriptor));

        if (newDescriptor.Lifetime != ServiceLifetime.Transient)
        {
            existingDescriptor.Lifetime = newDescriptor.Lifetime;
        }

        if (newDescriptor.ImplementationType is not null)
        {
            existingDescriptor.ImplementationType = newDescriptor.ImplementationType;
            existingDescriptor.Factory = null;
            existingDescriptor.Instance = null;
        }
        else if (newDescriptor.Factory is not null)
        {
            existingDescriptor.Factory = newDescriptor.Factory;
            existingDescriptor.ImplementationType = null;
            existingDescriptor.Instance = null;
        }
        else if (newDescriptor.Instance is not null)
        {
            existingDescriptor.Instance = newDescriptor.Instance;
            existingDescriptor.ImplementationType = null;
            existingDescriptor.Factory = null;
        }

        if (newDescriptor.Name is not null)
        {
            existingDescriptor.Name = newDescriptor.Name;
        }

        if (newDescriptor.Configure is not null)
        {
            existingDescriptor.Configure += newDescriptor.Configure;
        }

        if (newDescriptor.Condition is not null)
        {
            existingDescriptor.Condition += newDescriptor.Condition;
        }

        foreach (var param in newDescriptor.Parameters)
        {
            existingDescriptor.Parameters[param.Key] = param.Value;
        }

        foreach (var metadata in newDescriptor.Metadata)
        {
            existingDescriptor.Metadata[metadata.Key] = metadata.Value;
        }
    }
    #endregion

    // TryGetDescriptor
    internal ServiceDescriptor? TryGetDescriptor<TService>(string? name = null)
    {
        var descriptor = _descriptors.FirstOrDefault(binding => binding.BindingType == typeof(TService) && (name is null || binding.Name == name));
        return descriptor;
    }

    #region Service Binding
    internal class ServiceBindingBuilder : IServiceBuilder
    {
        private readonly ServiceConfigurator _configurator;
        private readonly ServiceDescriptor _descriptor;

        public ServiceDescriptor Descriptor => _descriptor;
        internal ServiceConfigurator Configurator => _configurator;

        public ServiceBindingBuilder(ServiceConfigurator configurator, ServiceDescriptor descriptor)
        {
            _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        /// <inheritdoc/>
        public IServiceBuilder To(Type implementationType)
        {
            Guard.NotNull(implementationType, nameof(implementationType));

            if (implementationType != Descriptor.BindingType && !implementationType.ImplementsInterface(Descriptor.BindingType))
            {
                throw new InvalidOperationException($"Type {implementationType.Name} does not implement type {Descriptor.BindingType.Name}.");
            }

            if (implementationType.IsInterface)
            {
                throw new InvalidOperationException("Cannot bind interfaces to themselves.");
            }

            if (implementationType.IsAbstract)
            {
                throw new InvalidOperationException("Cannot bind abstract types to themselves.");
            }

            if (Descriptor.Instance is not null)
            {
                Debug.WriteLine("Warning: Instance is already set. Setting implementation type will override the instance.");
                Descriptor.Instance = default;
            }

            if (Descriptor.Factory is not null)
            {
                Debug.WriteLine("Warning: Factory is already set. Setting implementation type will override the factory.");
                Descriptor.Factory = default;
            }

            if (implementationType.IsGenericTypeDefinition)
            {
#if OPEN_GENERIC_SUPPORTED
                if (!Descriptor.BindingType.TryGetGenericArguments(out var typeArguments) ||
                    !implementationType.TryMakeGenericType(typeArguments, out var constructedType))
                {
                    throw new InvalidOperationException("Cannot bind open generic types without specifying type arguments.");
                }
                else
                {
                    Descriptor.ImplementationType = constructedType;
                }
#else
                throw new NotSupportedException("Binding open generic types is not supported.");
#endif
            }
            else
            {
                Descriptor.ImplementationType = implementationType;
            }

            if (Descriptor.ImplementationType is not null)
            {
                Debug.WriteLine("Warning: Implementation type is already set. Setting implementation type will override the existing implementation type.");
            }
            else
            {
                Debug.WriteLine($"Binding service of type {Descriptor.BindingType.Name} to implementation {implementationType.Name}.");
            }

            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder AsSelf()
        {
            if (Descriptor.BindingType.IsAbstract || Descriptor.BindingType.IsInterface)
            {
                throw new InvalidOperationException("Cannot bind abstract types or interfaces to themselves.");
            }

            To(Descriptor.BindingType);

            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder WithInstance(object instance)
        {
            Guard.NotNull(instance, nameof(instance));

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
            Descriptor.Lifetime = ServiceLifetime.Singleton;
            Debug.WriteLine($"Setting instance of service {Descriptor.BindingType.Name}. Lifetime is set to singleton.");

            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder WithFactory(Func<IServiceProvider, object> factory)
        {
            Guard.NotNull(factory, nameof(factory));

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

        /// <inheritdoc/>
        public IServiceBuilder WithName(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            Descriptor.Name = name;
            Debug.WriteLine($"Setting name of service {Descriptor.BindingType.Name} to {name}.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder WithLifetime(ServiceLifetime lifetime)
        {
            Guard.InRange<ServiceLifetime>(lifetime, nameof(lifetime));

            Descriptor.Lifetime = lifetime;
            Debug.WriteLine($"Setting lifetime of service {Descriptor.BindingType.Name} to {lifetime}.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder AsSingleton()
        {
            Descriptor.Lifetime = ServiceLifetime.Singleton;
            Debug.WriteLine($"Setting lifetime of service {Descriptor.BindingType.Name} to singleton.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder AsScoped()
        {
            Descriptor.Lifetime = ServiceLifetime.Scoped;
            Debug.WriteLine($"Setting lifetime of service {Descriptor.BindingType.Name} to scoped.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder AsTransient()
        {
            Descriptor.Lifetime = ServiceLifetime.Transient;
            Debug.WriteLine($"Setting lifetime of service {Descriptor.BindingType.Name} to transient, which is default.  ");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder WithParameter(string name, object? value)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            if (Descriptor.Instance is not null)
            {
                throw new InvalidOperationException("Cannot specify parameters for an instance.");
            }

            if (Descriptor.Parameters.ContainsKey(name))
            {
                throw new InvalidOperationException($"Parameter with name {name} already exists.");
            }

            Descriptor.Parameters.Add(name, value);
            Debug.WriteLine($"Setting parameter {name} of service {Descriptor.BindingType.Name} to {value}.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder WithParameters(object parameters)
        {
            Guard.NotNull(parameters, nameof(parameters));

            if (parameters is not IReadOnlyDictionary<string, object?> dictionary)
            {
                // convert object into dictionary
                var properties = parameters?.GetType().GetProperties();
                dictionary = properties?.ToDictionary(property => property.Name, property => property.GetValue(parameters)).AsReadOnly()!;
            }

            if (dictionary is null)
            {
                throw new InvalidOperationException("Parameters must be a dictionary or an object with properties.");
            }

            return WithParameters(dictionary);
        }

        /// <inheritdoc/>
        public IServiceBuilder WithParameters(IReadOnlyDictionary<string, object?> parameters)
        {
            Guard.NotNull(parameters, nameof(parameters));

            foreach (var parameter in parameters)
            {
                if (Descriptor.Parameters.ContainsKey(parameter.Key))
                {
                    throw new InvalidOperationException($"Parameter with name {parameter.Key} already exists.");
                }

                Descriptor.Parameters.Add(parameter.Key, parameter.Value);
                Debug.WriteLine($"Setting parameter {parameter.Key} of service {Descriptor.BindingType.Name} to {parameter.Value}.");
            }

            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder WithMetadata(string name, object? value)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(value, nameof(value));

            if (Descriptor.Metadata.ContainsKey(name))
            {
                throw new InvalidOperationException($"Metadata with name {name} already exists.");
            }

            Descriptor.Metadata.Add(name, value);
            Debug.WriteLine($"Setting metadata {name} of service {Descriptor.BindingType.Name} to {value}.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder WithOptions<TOptions>(TOptions options)
            where TOptions : class
        {
            Guard.NotNull(options, nameof(options));

            if (Descriptor.Options is not null)
            {
                Debug.WriteLine("Warning: Options are already set. Setting options will override the existing options.");
            }

            // Check if options implement IOptions<T>
            if (options.GetType().IsGenericType && options.GetType().GetGenericTypeDefinition() == typeof(IOptions<>))
            {
                Descriptor.Options = options;
                Descriptor.OptionsType = options.GetType();
            }
            else
            {
                Descriptor.Options = new OptionsWrapper<TOptions>(options);
                Descriptor.OptionsType = typeof(OptionsWrapper<TOptions>);
            }

            Debug.WriteLine($"Setting options for service {Descriptor.BindingType.Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder Configure(Action<object> configure)
        {
            Guard.NotNull(configure, nameof(configure));

            Descriptor.Configure = service => configure(service);
            Debug.WriteLine($"Setting configuration for service {Descriptor.BindingType.Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder Configure<TService>(Action<TService> configure)
            where TService : class
        {
            Guard.NotNull(configure, nameof(configure));

            if (Descriptor.Configure is not null)
            {
                Debug.WriteLine("Warning: Configuration is already set. Setting configuration will override the existing configuration.");
            }

            Descriptor.Configure = service =>
            {
                if (service is not TService castedService)
                {
                    throw new InvalidOperationException($"Service is not of type {typeof(TService).Name}.");
                }

                configure(castedService);
            };

            Debug.WriteLine($"Setting configuration for service {Descriptor.BindingType.Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder Configure<TService, TOptions>(Action<TService, TOptions> configure)
            where TService : class
            where TOptions : class
        {
            Guard.NotNull(configure, nameof(configure));

            Descriptor.Configure = service =>
            {
                if (service is not TService castedService)
                {
                    throw new InvalidOperationException($"Service is not of type {typeof(TService).Name}.");
                }

                var options = Descriptor.Options as TOptions;

                if (options is null)
                {
                    throw new InvalidOperationException($"Options of type {typeof(TOptions).Name} not found.");
                }

                configure(castedService, options);
            };

            Debug.WriteLine($"Setting configuration for service {Descriptor.BindingType.Name}.");
            return this;
        }

        internal ServiceDescriptor GetDescriptor() => (_descriptor as ServiceDescriptor)!;
    }

    internal class ServiceBuilder<TService> : ServiceBindingBuilder, IServiceBuilder<TService> where TService : notnull
    {
        public ServiceBuilder(ServiceConfigurator configurator, ServiceDescriptor descriptor) : base(configurator, descriptor) { }

        /// <inheritdoc/>
        public IServiceBuilder<TService> To<TImplementation>() where TImplementation : class, TService
        {
            var implementationType = typeof(TImplementation);

            // Shouldn't be necessary to check this, but just in case
            if (implementationType.IsInterface)
            {
                throw new InvalidOperationException("Cannot bind interfaces to themselves.");
            }

            if (implementationType.IsAbstract)
            {
                throw new InvalidOperationException("Cannot bind abstract types to themselves.");
            }

            // Is the implementation type an illigal generic type?
            if (implementationType.IsOpenGeneric())
            {
                throw new InvalidOperationException("Cannot bind open generic types to themselves.");
            }

            if (Descriptor.Instance is not null)
            {
                Debug.WriteLine("Warning: Instance is already set. Setting implementation type will override the instance.");
                Descriptor.Instance = default;
            }

            if (Descriptor.Factory is not null)
            {
                Debug.WriteLine("Warning: Factory is already set. Setting implementation type will override the factory.");
                Descriptor.Factory = default;
            }

            if (Descriptor.ImplementationType is not null)
            {
                Debug.WriteLine("Warning: Implementation type is already set. Setting implementation type will override the existing implementation type.");
            }
            else
            {
                Debug.WriteLine($"Binding service of type {Descriptor.BindingType.Name} to implementation {implementationType.Name}.");
            }

            Descriptor.ImplementationType = implementationType;
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder<TService> WithFactory(Func<IServiceProvider, TService> factory)
        {
            Guard.NotNull(factory, nameof(factory));

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

        /// <inheritdoc/>
        public IServiceBuilder<TService> WithInstance(TService instance)
        {
            base.WithInstance(instance);
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder<TService> Configure(Action<TService> configure)
        {
            Guard.NotNull(configure, nameof(configure));

            Descriptor.Configure = service => configure((TService)service);

            Debug.WriteLine($"Setting configuration for service {Descriptor.BindingType.Name}.");
            return this;
        }

        /// <inheritdoc/>
        public IServiceBuilder<TService> Configure<TOptions>(Action<TService, TOptions> configure) where TOptions : class
        {
            Guard.NotNull(configure, nameof(configure));

            Descriptor.Configure = service =>
            {
                if (service is not TService castedService)
                {
                    throw new InvalidOperationException($"Service is not of type {typeof(TService).Name}.");
                }

                var options = Descriptor.Options as TOptions;

                if (options is null)
                {
                    throw new InvalidOperationException($"Options of type {typeof(TOptions).Name} not found.");
                }

                configure(castedService, options);
            };

            Debug.WriteLine($"Setting configuration for service {Descriptor.BindingType.Name}.");
            return this;
        }
    }
    #endregion
}

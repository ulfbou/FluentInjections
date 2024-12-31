// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Extensions;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Utils;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Diagnostics;

namespace FluentInjections.Internal.Configurators;

public abstract class ServiceConfigurator : IServiceConfigurator, IDisposable
{
    protected readonly List<ServiceBindingDescriptor> _bindings = new();
    protected readonly ILogger<ServiceConfigurator> _logger;

    internal IReadOnlyList<ServiceBindingDescriptor> Bindings => _bindings.AsReadOnly();
    public ConflictResolutionMode ConflictResolution { get; set; }

    public ServiceConfigurator(ConflictResolutionMode? mode = null, ILogger<ServiceConfigurator>? logger = null)
    {
        ConflictResolution = mode ?? ConflictResolutionMode.WarnAndReplace;
        _logger = logger ?? LoggerUtility.CreateLogger<ServiceConfigurator>();
    }

    // TODO: Handle open generic types
    /// <inheritdoc />
    public IServiceBinding<TService> Bind<TService>() where TService : notnull
    {
        var descriptor = new ServiceBindingDescriptor(typeof(TService));
        var existing_descriptor = _bindings.FirstOrDefault(binding => binding.BindingType == descriptor.BindingType && binding.Name == descriptor.Name);

        ResolveConflict(descriptor, existing_descriptor);

        _bindings.Add(descriptor);
        return new ServiceBinding<TService>(this, descriptor);
    }

    private void ResolveConflict(ServiceBindingDescriptor descriptor, ServiceBindingDescriptor? existingDescriptor)
    {
        if (existingDescriptor is not null)
        {
            switch (ConflictResolution)
            {
                case ConflictResolutionMode.Replace:
                    _bindings.Remove(existingDescriptor);
                    break;
                case ConflictResolutionMode.WarnAndReplace:
                    _logger.LogWarning($"Service of type {descriptor.BindingType.Name} already exists. Replacing with new descriptor.");
                    _bindings.Remove(existingDescriptor);
                    break;
                case ConflictResolutionMode.Prevent:
                    _logger.LogError($"Service of type {descriptor.BindingType.Name} already exists. Registration prevented.");
                    throw new InvalidOperationException($"Service of type {descriptor.BindingType.Name} already exists.");
                case ConflictResolutionMode.Merge:
                    MergeDescriptors(existingDescriptor, descriptor);
                    break;
                default:
                    throw new NotImplementedException($"Conflict resolution mode {ConflictResolution} not supported.");
            }
        }
    }

    private void MergeDescriptors(ServiceBindingDescriptor existingDescriptor, ServiceBindingDescriptor newDescriptor)
    {
        foreach (var param in newDescriptor.Parameters)
        {
            existingDescriptor.Parameters[param.Key] = param.Value;
        }

        foreach (var metadata in newDescriptor.Metadata)
        {
            existingDescriptor.Metadata[metadata.Key] = metadata.Value;
        }

        existingDescriptor.Configure += newDescriptor.Configure;
    }

    /// <summary>
    /// Tries to get the service binding for the specified type.
    /// </summary>
    /// <param name="type">The type of the service.</param>
    /// <returns>The service binding descriptor or <see langword="null"/> if not found.</returns>
    /// <remarks>
    /// This method is used internally to get the service binding for a specific type.
    /// </remarks>
    internal ServiceBindingDescriptor? TryGetDescriptor(Type type)
    {
        return _bindings.FirstOrDefault(descriptor => descriptor.BindingType == type);
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
    internal ServiceBindingDescriptor GetDescriptor(Type type)
    {
        ServiceBindingDescriptor? descriptor = TryGetDescriptor(type);

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

    protected abstract void Register(ServiceBindingDescriptor descriptor);

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
            Guard.NotNull(implementationType, nameof(implementationType));

            if (implementationType != Descriptor.BindingType && !implementationType.IsAssignableTo(Descriptor.BindingType))
            {
                throw new InvalidOperationException($"Type {implementationType.Name} is not assignable to {Descriptor.BindingType.Name}.");
            }

            if (implementationType != Descriptor.BindingType && !implementationType.IsAssignableTo(Descriptor.BindingType))
            {
                throw new InvalidOperationException($"Type {implementationType.Name} is not assignable to {Descriptor.BindingType.Name}.");
            }

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

        public IServiceBinding<TService> AsSelf()
        {
            if (Descriptor.BindingType.IsAbstract || Descriptor.BindingType.IsInterface)
            {
                throw new InvalidOperationException("Cannot bind abstract types or interfaces to themselves.");
            }

            To(Descriptor.BindingType);

            return this;
        }

        public IServiceBinding<TService> WithInstance(TService instance)
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

        public IServiceBinding<TService> WithFactory(Func<IServiceProvider, TService> factory)
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

        public IServiceBinding<TService> WithKey(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            Descriptor.Name = name;
            Debug.WriteLine($"Setting name of service {Descriptor.BindingType.Name} to {name}.");
            return this;
        }

        public IServiceBinding<TService> WithLifetime(ServiceLifetime lifetime)
        {
            // Check if the lifetime is out of range
            if (lifetime < ServiceLifetime.Singleton || lifetime > ServiceLifetime.Transient)
            {
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Lifetime must be within the range of the enumeration.");
            }

            Descriptor.Lifetime = lifetime;
            Debug.WriteLine($"Setting lifetime of service {Descriptor.BindingType.Name} to {lifetime}.");
            return this;
        }

        public IServiceBinding<TService> AsSingleton()
        {
            Descriptor.Lifetime = ServiceLifetime.Singleton;
            Debug.WriteLine($"Setting lifetime of service {Descriptor.BindingType.Name} to singleton.");
            return this;
        }

        public IServiceBinding<TService> AsScoped()
        {
            Descriptor.Lifetime = ServiceLifetime.Scoped;
            Debug.WriteLine($"Setting lifetime of service {Descriptor.BindingType.Name} to scoped.");
            return this;
        }

        public IServiceBinding<TService> AsTransient()
        {
            Descriptor.Lifetime = ServiceLifetime.Transient;
            Debug.WriteLine($"Setting lifetime of service {Descriptor.BindingType.Name} to transient, which is default.  ");
            return this;
        }

        public IServiceBinding<TService> WithParameter(string name, object value)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(value, nameof(value));

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

        public IServiceBinding<TService> WithParameters(object parameters)
        {
            if (parameters is not IReadOnlyDictionary<string, object> dictionary)
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

        public IServiceBinding<TService> WithParameters(IReadOnlyDictionary<string, object> parameters)
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

        public IServiceBinding<TService> WithMetadata(string name, object value)
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

        public IServiceBinding<TService> Configure(Action<TService> configure)
        {
            Descriptor.Configure = service => configure((TService)service);
            Debug.WriteLine($"Setting configuration for service {Descriptor.BindingType.Name}.");
            return this;
        }

        internal ServiceBindingDescriptor<TService> GetDescriptor() => (_descriptor as ServiceBindingDescriptor<TService>)!;
    }

    public void Dispose()
    {
        _bindings.Clear();
    }
}

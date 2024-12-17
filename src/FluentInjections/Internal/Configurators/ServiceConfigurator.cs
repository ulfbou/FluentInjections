using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FluentInjections.Validation;
using System.Diagnostics;

namespace FluentInjections.Internal.Configurators;

internal class ServiceConfigurator : IServiceConfigurator
{
    private readonly IServiceCollection _services;

    public ServiceConfigurator(IServiceCollection services)
    {
        _services = services;
    }

    public IServiceBinding<TService> Bind<TService>() where TService : class
    {
        return new ServiceBinding<TService>(_services);
    }

    public void Unbind<T>()
    {
        var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(T));

        if (descriptor is null)
        {
            throw new InvalidOperationException($"The service {typeof(T).Name} is not registered.");
        }

        _services.Remove(descriptor);
    }

    private class ServiceBinding<TService> : IServiceBinding<TService> where TService : class
    {
        private readonly IServiceCollection _services;
        private readonly Type _serviceType;
        private Type? _implementationType;
        private IReadOnlyDictionary<string, object>? _parameters;
        private ServiceLifetime _lifetime = ServiceLifetime.Transient;
        private string? _name;
        private TService? _instance;
        private Func<IServiceProvider, TService>? _factory;
        private Action<TService>? _configure;

        public ServiceBinding(IServiceCollection services)
        {
            _services = services;
            _serviceType = typeof(TService);
        }

        public IServiceBinding<TService> To<TImplementation>() where TImplementation : class, TService
        {
            // Check if the implementation has been registered before. If so, remove it and log information. 
            if (_services.Any(d => d.ServiceType == typeof(TImplementation)))
            {
                var descriptor = _services.First(d => d.ServiceType == typeof(TImplementation));

                _services.Remove(descriptor);
                Debug.WriteLine($"Replacing service {_serviceType.Name} implementation {descriptor.ImplementationType!.Name} with {typeof(TImplementation).Name}.");
            }
            else if (_implementationType is not null)
            {
                Debug.WriteLine($"Replacing service {_serviceType.Name} implementation {_implementationType.Name} to {typeof(TImplementation).Name}.");
            }
            else
            {
                Debug.WriteLine($"Binding service {_serviceType.Name} implementation to {typeof(TImplementation).Name}.");
            }

            _implementationType = typeof(TImplementation);
            return this;
        }

        public IServiceBinding<TService> To(Type implementationType)
        {
            ArgumentGuard.NotNull(implementationType, nameof(implementationType));

            // TODO: check if _implementationType is set and log a warning.

            if (!implementationType.IsAbstract && !implementationType.IsInterface)
            {
                throw new ArgumentException($"{implementationType.Name} must be a non-abstract class.");
            }

            if (implementationType == _serviceType && !implementationType.IsAssignableFrom(_serviceType))
            {
                throw new ArgumentException($"{implementationType.Name} must implement {_serviceType.Name}.");
            }

            // Check if the implementation has been registered before. If so, remove it and log information.

            if (_services.Any(d => d.ServiceType == implementationType))
            {
                var descriptor = _services.First(d => d.ServiceType == implementationType);

                _services.Remove(descriptor);
                Debug.WriteLine($"Replacing service {_serviceType.Name} implementation {descriptor.ImplementationType!.Name} with {implementationType.Name}.");
            }
            else if (_implementationType is not null)
            {
                Debug.WriteLine($"Replacing service {_serviceType.Name} implementation {_implementationType.Name} to {implementationType.Name}.");
            }
            else
            {
                Debug.WriteLine($"Binding service {_serviceType.Name} implementation to {implementationType.Name}.");
            }

            _implementationType = implementationType;

            return this;
        }

        public IServiceBinding<TService> AsSelf()
        {
            _implementationType = _serviceType;
            Debug.WriteLine($"Binding {_serviceType.Name} to itself.");
            return this;
        }

        public IServiceBinding<TService> AsSingleton()
        {
            _lifetime = ServiceLifetime.Singleton;
            Debug.WriteLine($"Binding {_serviceType.Name} as a singleton.");
            return this;
        }

        public IServiceBinding<TService> AsScoped()
        {
            _lifetime = ServiceLifetime.Scoped;
            Debug.WriteLine($"Binding {_serviceType.Name} as scoped.");
            return this;
        }

        public IServiceBinding<TService> AsTransient()
        {
            _lifetime = ServiceLifetime.Transient;
            Debug.WriteLine($"Binding {_serviceType.Name} as transient.");
            return this;
        }

        public IServiceBinding<TService> WithLifetime(ServiceLifetime lifetime)
        {
            _lifetime = lifetime;
            Debug.WriteLine($"Binding {_serviceType.Name} with lifetime {lifetime}.");
            return this;
        }

        public IServiceBinding<TService> WithInstance(TService instance)
        {
            ArgumentGuard.NotNull(instance, nameof(instance));

            // TODO: Should we throw if _implementationType, _factory, _instance or _parameters are already set?
            // TODO: Consider if we should require a call to AsSelf() before WithInstance(), and throw if not called.
            // TODO: Consider if Singleton should be the default lifetime if WithInstance() is called.

            if (_instance is not null)
            {
                Debug.WriteLine($"Replacing {_serviceType.Name} instance.");
            }
            else
            {
                Debug.WriteLine($"Binding {_serviceType.Name} to instance.");
            }

            _instance = instance;
            return this;
        }

        public IServiceBinding<TService> WithFactory(Func<IServiceProvider, TService> factory)
        {
            ArgumentGuard.NotNull(factory, nameof(factory));

            _factory = factory;
            Debug.WriteLine($"Binding {_serviceType.Name} with factory.");
            return this;
        }

        public IServiceBinding<TService> WithParameters(object parameters)
        {
            ArgumentGuard.NotNull(parameters, nameof(parameters));

            if (parameters is IReadOnlyDictionary<string, object> dictionary)
            {
                _parameters = dictionary;
                Debug.WriteLine($"Binding {_serviceType.Name} with parameters: {string.Join(", ", dictionary.Keys)}.");
                return this;
            }

            var properties = parameters.GetType().GetProperties();
            var dictionaryBuilder = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                dictionaryBuilder[property.Name] = property.GetValue(parameters)!;
            }

            _parameters = dictionaryBuilder;
            Debug.WriteLine($"Binding {_serviceType.Name} with parameters: {string.Join(", ", dictionaryBuilder.Keys)}.");

            return this;
        }

        public IServiceBinding<TService> WithParameters(IReadOnlyDictionary<string, object> parameters)
        {
            ArgumentGuard.NotNull(parameters, nameof(parameters));

            _parameters = parameters;
            Debug.WriteLine($"Binding {_serviceType.Name} with parameters: {string.Join(", ", parameters.Keys)}.");
            return this;
        }

        public IServiceBinding<TService> WithName(string name)
        {
            ArgumentGuard.NotNull(name, nameof(name));

            _name = name;
            return this;
        }

        public IServiceBinding<TService> Configure(Action<TService> configure)
        {
            ArgumentGuard.NotNull(configure, nameof(configure));

            _configure = configure;
            Debug.WriteLine($"Configuring {_serviceType.Name}.");
            return this;
        }

        public IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TOptions> configure) where TOptions : class
        {
            ArgumentGuard.NotNull(configure, nameof(configure));

            _services.Configure(configure);
            Debug.WriteLine($"Configuring {_serviceType.Name} options.");
            return this;
        }

        public IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TService, TOptions> configure) where TOptions : class
        {
            ArgumentGuard.NotNull(configure, nameof(configure));

            _services.Configure<TOptions>(options =>
            {
                if (_instance != null)
                {
                    configure(_instance, options);
                }
            });

            Debug.WriteLine($"Configuring {_serviceType.Name} options.");

            return this;
        }

        public IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TService, TOptions, IServiceConfigurator> configure) where TOptions : class
        {
            ArgumentGuard.NotNull(configure, nameof(configure));

            _services.Configure<TOptions>(options =>
            {
                if (_instance != null)
                {
                    configure(_instance, options, new ServiceConfigurator(_services));
                }
            });

            Debug.WriteLine($"Configuring {_serviceType.Name} options.");

            return this;
        }

        public void Register()
        {
            if (_implementationType is null && _instance is null && _factory is null)
            {
                throw new InvalidOperationException("No implementation, instance, or factory provided for the service.");
            }

            if (_instance is not null)
            {
                if (_configure is not null)
                {
                    _configure(_instance);
                }

                _services.Add(new ServiceDescriptor(_serviceType, _instance));
            }
            else if (_factory is not null)
            {
                _services.Add(new ServiceDescriptor(
                    _serviceType,
                    provider => _factory(provider),
                    _lifetime));
            }
            else if (_implementationType is not null)
            {
                _services.Add(new ServiceDescriptor(
                    _serviceType,
                    provider => CreateInstance(provider),
                    _lifetime));
            }
        }

        private TService CreateInstance(IServiceProvider provider)
        {
            if (_factory is not null)
            {
                return _factory(provider);
            }

            if (_parameters is null)
            {
                return (TService)ActivatorUtilities.CreateInstance(provider, _implementationType!);
            }

            var parameterDictionary = _parameters as IDictionary<string, object>;

            if (parameterDictionary == null)
            {
                throw new InvalidOperationException("Parameters must be a dictionary.");
            }

            // TODO: Handle the case where WithParameters provide partial parameters, and the rest are resolved from the container.

            return (TService)ActivatorUtilities.CreateInstance(
                provider,
                _implementationType!,
                ResolveParameters(provider, parameterDictionary));
        }

        private object[] ResolveParameters(IServiceProvider provider, IDictionary<string, object> parameters)
        {
            var constructor = _implementationType!.GetConstructors().SingleOrDefault();

            if (constructor is null)
            {
                throw new InvalidOperationException($"No public constructors found for {_implementationType.Name}.");
            }

            var resolvedParameters = constructor.GetParameters()
                .Select(param =>
                {
                    if (parameters.TryGetValue(param.Name!, out var explicitValue))
                    {
                        return explicitValue;
                    }

                    // Resolve from the DI container if not explicitly provided
                    var service = provider.GetService(param.ParameterType);

                    // If the parameter is required and can't be resolved, throw an exception
                    if (service == null && !param.IsOptional)
                    {
                        throw new ArgumentException(
                            $"The parameter '{param.Name}' of type '{param.ParameterType.Name}' could not be resolved.");
                    }

                    // Use default value for optional parameters
                    return service ?? param.DefaultValue;
                })
                .ToArray();

            return resolvedParameters!;
        }
    }
}

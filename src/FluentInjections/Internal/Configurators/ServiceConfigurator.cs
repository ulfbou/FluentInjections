using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace FluentInjections.Internal.Configurators
{
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

        private class ServiceBinding<TService>(IServiceCollection services) : IServiceBinding<TService> where TService : class
        {
            private readonly IServiceCollection _services = services;
            private Type? _implementationType;
            private object? _parameters;
            private ServiceLifetime _lifetime = ServiceLifetime.Transient;
            private string? _name;
            private TService? _instance;
            private Func<TService>? _factory;
            private Action<TService>? _configure;

            public IServiceBinding<TService> To<TImplementation>() where TImplementation : class, TService
            {
                _implementationType = typeof(TImplementation);
                return this;
            }

            public IServiceBinding<TService> As<TImplementation>() where TImplementation : class, TService => To<TImplementation>();

            public IServiceBinding<TService> WithParameters(object parameters)
            {
                _parameters = parameters;
                return this;
            }

            public IServiceBinding<TService> WithLifetime(ServiceLifetime lifetime)
            {
                _lifetime = lifetime;
                return this;
            }

            public IServiceBinding<TService> WithName(string name)
            {
                _name = name;
                return this;
            }

            public IServiceBinding<TService> AsSelf()
            {
                _services.Add(new ServiceDescriptor(typeof(TService), provider => CreateInstance(provider), _lifetime));
                return this;
            }

            public IServiceBinding<TService> WithInstance(TService instance)
            {
                _instance = instance;
                _services.Add(new ServiceDescriptor(typeof(TService), instance));
                return this;
            }

            public IServiceBinding<TService> WithFactory(Func<TService> factory)
            {
                _factory = factory;
                _services.Add(new ServiceDescriptor(typeof(TService), provider => factory(), _lifetime));
                return this;
            }

            public IServiceBinding<TService> Configure(Action<TService> configure)
            {
                _configure = configure;
                return this;
            }

            public IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TOptions> configure) where TOptions : class
            {
                _services.Configure(configure);
                return this;
            }

            public IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TService, TOptions> configure) where TOptions : class
            {
                _services.Configure<TOptions>(options =>
                {
                    if (_instance != null)
                        configure(_instance, options);
                });
                return this;
            }

            public IServiceBinding<TService> ConfigureOptions<TOptions>(Action<TService, TOptions, IServiceConfigurator> configure) where TOptions : class
            {
                _services.Configure<TOptions>(options =>
                {
                    if (_instance != null)
                        configure(_instance, options, new ServiceConfigurator(_services));
                });
                return this;
            }

            public void Register()
            {
                if (_implementationType is null && _instance is null && _factory is null)
                {
                    throw new InvalidOperationException("No implementation, instance, or factory provided for the service.");
                }

                if (_implementationType is not null)
                {
                    _services.Add(new ServiceDescriptor(
                        typeof(TService),
                        provider => CreateInstance(provider),
                        _lifetime));
                }

                if (_configure is not null && _instance is not null)
                {
                    _configure(_instance);
                }
            }

            private TService CreateInstance(IServiceProvider provider)
            {
                if (_factory is not null)
                {
                    return _factory();
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

                return (TService)ActivatorUtilities.CreateInstance(
                    provider,
                    _implementationType!,
                    ResolveParameters(provider, parameterDictionary));
            }

            private object[] ResolveParameters(IServiceProvider provider, IDictionary<string, object> parameters)
            {
                var resolvedParameters = new List<object>();

                foreach (var parameter in parameters)
                {
                    // TODO: Handle named, unresolvable, and optional parameters. 
                    resolvedParameters.Add(parameter.Value ?? provider.GetService(parameter.Key.GetType())!);
                }

                return resolvedParameters.ToArray();
            }
        }
    }
}

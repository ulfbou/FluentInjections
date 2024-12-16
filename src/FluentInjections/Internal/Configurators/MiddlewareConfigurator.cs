using System.Net.Http;

using Autofac;

using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentInjections.Internal.Configurators
{
    internal class MiddlewareConfigurator<TBuilder> : IMiddlewareConfigurator<TBuilder>
    {
        private readonly IServiceCollection _services;
        public TBuilder Builder { get; }

        public MiddlewareConfigurator(IServiceCollection services, TBuilder builder)
        {
            _services = services;
            Builder = builder;
        }

        public IMiddlewareBinding<TMiddleware, TBuilder> Bind<TMiddleware>() where TMiddleware : class
        {
            return new MiddlewareBinding<TMiddleware, TBuilder>(_services, this);
        }

        private class MiddlewareBinding<TMiddleware, TOuterBuilder> : IMiddlewareBinding<TMiddleware, TOuterBuilder> where TMiddleware : class
        {
            private readonly IServiceCollection _services;
            private readonly IMiddlewareConfigurator<TOuterBuilder> _configurator;
            private Type? _implementationType;
            private object? _parameters;
            private ServiceLifetime _lifetime = ServiceLifetime.Transient;
            private Action<TMiddleware>? _configure;

            public MiddlewareBinding(IServiceCollection services, IMiddlewareConfigurator<TOuterBuilder> configurator)
            {
                _services = services ?? throw new ArgumentNullException(nameof(services));
                _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
            }

            public IMiddlewareBinding<TMiddleware, TOuterBuilder> To<TImplementation>() where TImplementation : class, TMiddleware
            {
                _implementationType = typeof(TImplementation);
                return this;
            }

            public IMiddlewareBinding<TMiddleware, TOuterBuilder> WithParameters(object parameters)
            {
                ArgumentGuard.NotNull(parameters, nameof(parameters));
                _parameters = parameters;
                return this;
            }

            public IMiddlewareBinding<TMiddleware, TOuterBuilder> WithLifetime(ServiceLifetime lifetime)
            {
                _lifetime = lifetime;
                return this;
            }

            public IMiddlewareBinding<TMiddleware, TOuterBuilder> Configure(Action<TMiddleware> configure)
            {
                _configure = configure;
                return this;
            }

            public IMiddlewareBinding<TMiddleware, TOuterBuilder> ConfigureOptions<TOptions>(Action<TOptions> configure) where TOptions : class
            {
                _services.Configure(configure);
                return this;
            }

            public IMiddlewareBinding<TMiddleware, TOuterBuilder> ConfigureOptions<TOptions>(Action<TMiddleware, TOptions> configure) where TOptions : class
            {
                _services.Configure<TOptions>(options =>
                {
                    var instance = (TMiddleware)Activator.CreateInstance(typeof(TMiddleware))!;
                    configure(instance, options);
                });
                return this;
            }

            public IMiddlewareBinding<TMiddleware, TOuterBuilder> ConfigureOptions<TOptions>(Action<TMiddleware, TOptions, IMiddlewareConfigurator<TOuterBuilder>> configure) where TOptions : class
            {
                _services.Configure<TOptions>(options =>
                {
                    var instance = (TMiddleware)Activator.CreateInstance(typeof(TMiddleware))!;
                    configure(instance, options, _configurator);
                });
                return this;
            }

            public void Register()
            {
                if (_implementationType == null)
                {
                    throw new InvalidOperationException("No implementation provided for the middleware.");
                }

                _services.Add(new ServiceDescriptor(
                    typeof(TMiddleware),
                    provider => CreateInstance(provider),
                    _lifetime));

                if (_configure != null)
                {
                    var instance = (TMiddleware)Activator.CreateInstance(_implementationType)!;
                    _configure(instance);
                }
            }

            private TMiddleware CreateInstance(IServiceProvider provider)
            {
                if (_parameters == null)
                {
                    return (TMiddleware)ActivatorUtilities.CreateInstance(provider, _implementationType!);
                }

                var parameterDictionary = _parameters as IDictionary<string, object>;
                if (parameterDictionary == null)
                {
                    throw new InvalidOperationException("Parameters must be a dictionary.");
                }

                return (TMiddleware)ActivatorUtilities.CreateInstance(
                    provider,
                    _implementationType!,
                    ResolveParameters(provider, parameterDictionary));
            }

            private object[] ResolveParameters(IServiceProvider provider, IDictionary<string, object> parameters)
            {
                var resolvedParameters = new List<object>();

                foreach (var parameter in parameters)
                {
                    resolvedParameters.Add(parameter.Value ?? provider.GetService(parameter.Key.GetType())!);
                }

                return resolvedParameters.ToArray();
            }
        }
    }
}

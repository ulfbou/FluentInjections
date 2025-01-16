using FluentInjections.Extensions;
using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Helpers;
using FluentInjections.Internal.Utils;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Reflection;

namespace FluentInjections.Internal;

public static class FluentInjectionsContext
{
    public static IFluentInjectionsBuilder CreateApplicationBuilder(params object[] parameters)
    {
        string[] args = parameters.Where(p => p is string).Cast<string>().ToArray();
        Assembly[]? assemblies = parameters.OfType<Assembly>().ToArray();
        return new FluentInjectionsBuilder(WebApplication.CreateBuilder(args), assemblies);
    }

    public static IFluentInjectionsBuilder Create(WebApplicationBuilder builder, params Assembly[]? assemblies)
    {
        return new FluentInjectionsBuilder(builder, assemblies);
    }

    internal class FluentInjectionsBuilder : IFluentInjectionsBuilder
    {
        private readonly WebApplicationBuilder _builder;
        private readonly IServiceConfigurator _serviceConfigurator;
        private readonly Assembly[]? _assemblies;

        public FluentInjectionsBuilder(WebApplicationBuilder builder, Assembly[]? assemblies = null)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _assemblies = assemblies ?? AppDomain.CurrentDomain.GetAssemblies();
            _serviceConfigurator = new NetCoreServiceConfigurator(_builder.Services, LoggerUtility.CreateLogger<NetCoreServiceConfigurator>());
        }

        /// <inheritdoc />
        public IConfigurationManager Configuration => ((IHostApplicationBuilder)_builder).Configuration;

        public IHostEnvironment Environment => ((IHostApplicationBuilder)_builder).Environment;

        public ILoggingBuilder Logging => _builder.Logging;

        public IMetricsBuilder Metrics => _builder.Metrics;

        public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_builder).Properties;

        public IServiceCollection Services => _builder.Services;

        public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null)
            where TContainerBuilder : notnull
            => ((IHostApplicationBuilder)_builder).ConfigureContainer(factory, configure);

        public IFluentInjectionsApplication Build()
        {
            var modules = DiscoveryHelper.DiscoverModules<IServiceConfigurator>(_assemblies);
            RegisterHelper.RegisterModules<IServiceConfigurator>(modules, _serviceConfigurator);
            _serviceConfigurator.Register();
            return new FluentInjectionsApplication(_builder.Build());
        }
    }

    internal class FluentInjectionsApplication : IFluentInjectionsApplication
    {
        private readonly WebApplication _app;
        private readonly IMiddlewareConfigurator _middlewareConfigurator;

        public FluentInjectionsApplication(WebApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _middlewareConfigurator = new NetCoreMiddlewareConfigurator(_app, _app.Services, LoggerUtility.CreateLogger<NetCoreMiddlewareConfigurator>());
        }

        /// <inheritdoc/>
        public IServiceProvider Services => _app.Services;

        /// <inheritdoc/>
        public IServiceProvider ApplicationServices { get => ((IApplicationBuilder)_app).ApplicationServices; set => ((IApplicationBuilder)_app).ApplicationServices = value; }

        /// <inheritdoc/>
        public IFeatureCollection ServerFeatures => ((IApplicationBuilder)_app).ServerFeatures;

        /// <inheritdoc/>
        public IDictionary<string, object?> Properties => ((IApplicationBuilder)_app).Properties;

        /// <inheritdoc/>
        public IServiceProvider ServiceProvider => ((IEndpointRouteBuilder)_app).ServiceProvider;

        /// <inheritdoc/>
        public ICollection<EndpointDataSource> DataSources => ((IEndpointRouteBuilder)_app).DataSources;

        /// <inheritdoc/>
        public RequestDelegate Build()
        {
            var middlewareModules = DiscoveryHelper.DiscoverModules<IMiddlewareConfigurator>();

            foreach (var moduleType in middlewareModules)
            {
                RegisterHelper.RegisterModule(moduleType, typeof(IMiddlewareConfigurator), _middlewareConfigurator);
            }

            _middlewareConfigurator.Register();
            return ((IApplicationBuilder)_app).Build();
        }

        public IApplicationBuilder CreateApplicationBuilder() => ((IEndpointRouteBuilder)_app).CreateApplicationBuilder();
        public void Dispose() => ((IDisposable)_app).Dispose();
        public ValueTask DisposeAsync() => _app.DisposeAsync();
        public IApplicationBuilder New() => ((IApplicationBuilder)_app).New();
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            var middlewareModules = DiscoveryHelper.DiscoverModules<IMiddlewareConfigurator>();

            foreach (var moduleType in middlewareModules)
            {
                RegisterHelper.RegisterModule(moduleType, typeof(IMiddlewareConfigurator), _middlewareConfigurator);
            }

            _middlewareConfigurator.Register();
            return _app.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken = default) => _app.StopAsync(cancellationToken);

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            // TODO: Implement middleware registration logic via the middleware configurator.
            // - ILifetimeCycle events
            return _app.Use(middleware);
        }

        public IApplicationBuilder UseMiddleware<TMiddleware>()
        {
            return _app.UseMiddleware<TMiddleware>();
        }

        public IApplicationBuilder UseMiddleware(Type middlewareType, params object[] parameters)
        {
            return _app.UseMiddleware(middlewareType, parameters);
        }
    }
}

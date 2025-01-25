using FluentInjections.Internal.Managers;
using FluentInjections.Internal.Utils;
using FluentInjections.Internal.Wrappers;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Reflection;

namespace FluentInjections;

public sealed class InjectionBuilder
{
    internal IServiceCollection _services = new ServiceCollection();
    internal IServiceProvider _serviceProvider;

    internal static readonly Lazy<InjectionBuilder> _instance = new Lazy<InjectionBuilder>(() => new InjectionBuilder());

    internal readonly Lazy<ServiceManager> _serviceManager =
        new Lazy<ServiceManager>(() => new ServiceManager(Instance._serviceProvider.GetRequiredService<IServiceCollection>()));

    internal readonly Lazy<WebApplicationBuilder> _hostBuilder =
        new Lazy<WebApplicationBuilder>(() => WebApplication.CreateBuilder());

    internal static InjectionBuilder Instance => _instance.Value;

    internal readonly ILoggerFactory _loggerFactory;

    public InjectionBuilder(IServiceProvider? serviceProvider = null, ILoggerFactory? loggerFactory = null)
    {
        _serviceProvider = serviceProvider ?? new ServiceCollection().BuildServiceProvider();
        _loggerFactory = loggerFactory ?? CreateCustomLoggerFactory();
    }

    public void RegisterCoreServices()
    {
        _serviceProvider.GetService<IServiceCollection>()?.AddSingleton<ILoggerFactory>(_ => CreateCustomLoggerFactory());
    }

    public void RegisterServices()
    {
        RegisterCoreServices();
        IServiceCollection services = _serviceProvider.GetService<IServiceCollection>()!;
        services?.AddSingleton<ServiceManager>(provider => new ServiceManager(services));
    }

    private ILoggerFactory CreateCustomLoggerFactory()
    {
        return _serviceProvider.GetService<ILoggerFactory>() ?? LoggerFactory.Create(builder => builder.AddConsole());
    }

    private ILogger<T> CreateCustomLogger<T>()
    {
        var loggerFactory = _serviceProvider.GetService<ILoggerFactory>() ?? CreateCustomLoggerFactory();
        return loggerFactory.CreateLogger<T>();
    }

    public static IInjectionBuilder For<TBuilder>()
    {
        var instance = Instance;
        var builderType = typeof(TBuilder);

        // Validate TBuilder (optional)
        // if builderType is an WebApplicationBuilder, return a WebApplicationBuilderWrapper
        if (builderType == typeof(WebApplicationBuilder))
        {
            return new WebApplicationInjectionBuilder(WebApplication.CreateBuilder());
        }

        throw new InvalidOperationException("Invalid builder type.");
    }

    internal sealed class WebApplicationInjectionBuilder : IInjectionBuilder<WebApplicationBuilderWrapper>
    {
        private AssemblyCollection _serviceAssemblies = new();
        private AssemblyCollection _middlewareAssemblies = new();
        private IServiceCollection? _serviceCollection;
        private IServiceProvider? _serviceProvider;
        private readonly WebApplicationBuilder _applicationBuilder;

        /// <inheritdoc />
        public IInjectionBuilder WithServices(params Assembly[]? assemblies)
        {
            _serviceAssemblies.AddRange(assemblies ?? AppDomain.CurrentDomain.GetAssemblies());
            return this;
        }

        /// <inheritdoc />
        public IInjectionBuilder WithServices(Action<AssemblyCollection> configureAssemblies)
        {
            Guard.NotNull(configureAssemblies, nameof(configureAssemblies));
            configureAssemblies.Invoke(_serviceAssemblies);
            return this;
        }

        /// <inheritdoc />
        public IInjectionBuilder WithMiddlewares(params Assembly[]? assemblies)
        {
            _middlewareAssemblies.AddRange(assemblies ?? AppDomain.CurrentDomain.GetAssemblies());
            return this;
        }

        public IInjectionBuilder WithMiddlewares(Action<AssemblyCollection> configureAssemblies)
        {
            Guard.NotNull(configureAssemblies, nameof(configureAssemblies));
            configureAssemblies.Invoke(_middlewareAssemblies);
            return this;
        }

        public IInjectionBuilder WithServiceCollection(IServiceCollection services)
        {
            Guard.NotNull(services, nameof(services));
            _serviceCollection = services;
            return this;
        }

        public IInjectionBuilder WithServiceProvider(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceCollection = serviceProvider.GetRequiredService<IServiceCollection>();
            _serviceProvider = serviceProvider;
            return this;
        }


        public WebApplicationInjectionBuilder() : base()
        {
            _applicationBuilder = WebApplication.CreateBuilder();
        }

        public WebApplicationInjectionBuilder(WebApplicationBuilder applicationBuilder) : base()
        {
            Guard.NotNull(applicationBuilder, nameof(applicationBuilder));
            _applicationBuilder = applicationBuilder;
        }

        public WebApplicationBuilderWrapper Build()
        {
            return new WebApplicationBuilderWrapper(_applicationBuilder, Instance._loggerFactory.CreateLogger<WebApplicationBuilderWrapper>());
        }
    }
}

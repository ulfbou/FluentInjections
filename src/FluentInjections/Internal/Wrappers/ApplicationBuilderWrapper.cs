// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using FluentInjections.Internal.Utils;
using System.Diagnostics;
using System.Linq;
using FluentInjections.Internal.Constants;
using Microsoft.Win32;

namespace FluentInjections.Internal.Wrappers;

public class ApplicationBuilderWrapper : IApplicationBuilder
{
    protected readonly IApplicationBuilder _innerBuilder;
    protected readonly ILogger<ApplicationBuilderWrapper> _logger;
    protected readonly ConcurrentBag<MiddlewareDescriptor> _middlewareDescriptors = new();
    protected readonly Lock _moduleRegistrationLock = new();
    protected readonly List<ModuleDescriptor> _registeredModules = new();
    protected readonly ConcurrentDictionary<Type, Type> _moduleRegistrations = new();
    protected readonly List<ModuleDescriptor> _sortedModules = new();
    protected int _currentModulePriority;

    internal NetCoreMiddlewareConfigurator Configurator { get; }

    /// <inheritdoc/>
    public IServiceProvider ApplicationServices { get => _innerBuilder.ApplicationServices; set => _innerBuilder.ApplicationServices = value; }

    /// <inheritdoc/>
    public IFeatureCollection ServerFeatures => _innerBuilder.ServerFeatures;

    /// <inheritdoc/>
    public IDictionary<string, object?> Properties => _innerBuilder.Properties;

    public ApplicationBuilderWrapper(IApplicationBuilder innerBuilder, ILogger<ApplicationBuilderWrapper> logger, IMiddlewareConfigurator? configurator = null)
    {
        _innerBuilder = innerBuilder ?? throw new ArgumentNullException(nameof(innerBuilder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Configurator = configurator as NetCoreMiddlewareConfigurator
            ?? new NetCoreMiddlewareConfigurator(this, this.ApplicationServices, LoggerUtility.CreateLogger<NetCoreMiddlewareConfigurator>());
    }

    private void Register(MiddlewareDescriptor descriptor)
    {
        Debug.WriteLine($"Registering Middleware {descriptor.Id}: {descriptor.MiddlewareType.Name} Priority: {descriptor.Priority}");
        _middlewareDescriptors.Add(descriptor);
    }

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        using (_moduleRegistrationLock.EnterScope())
        {
            Register(new MiddlewareDescriptor(middleware.GetType(), Configurator)
            {
                Instance = middleware,
                Priority = _currentModulePriority
            });

            return this;
        }
    }

    public IApplicationBuilder UseMiddleware<TMiddleware>()
    {
        using (_moduleRegistrationLock.EnterScope())
        {
            Register(new MiddlewareDescriptor(typeof(TMiddleware), Configurator)
            {
                Priority = _currentModulePriority
            });

            return this;
        }
    }

    public IApplicationBuilder Use(Action<IApplicationBuilder> configure)
    {
        try
        {
            using (_moduleRegistrationLock.EnterScope())
            {
                configure(this);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring application builder");
            // TODO: Handle configuration errors (e.g., log, fallback)
        }

        return this;
    }

    public void RegisterModule<TModule>(TModule module) where TModule : IModule<IMiddlewareConfigurator>
    {
        using (_moduleRegistrationLock.EnterScope())
        {
            _registeredModules.Add(new ModuleDescriptor
            {
                ModuleType = typeof(TModule),
                Instance = module
            });
        }
    }

    public async Task<RequestDelegate> BuildAsync()
    {
        // 1. Sort Modules
        using (_moduleRegistrationLock.EnterScope())
        {
            _sortedModules.AddRange(_registeredModules.OrderBy(m => m.Priority)
                                                      .ThenBy(m => _registeredModules.IndexOf(m)));
        }

        // 2. Execute Modules Concurrently
        var tasks = _sortedModules.Select(moduleDescriptor =>
        {
            try
            {
                using (_moduleRegistrationLock.EnterScope())
                {
                    _currentModulePriority = moduleDescriptor.Priority;
                    var module = moduleDescriptor.TryGet<IConfigurableModule<IMiddlewareConfigurator>>()
                        ?? moduleDescriptor.TryGet<IMiddlewareModule>();

                    if (module is null)
                    {
                        Debug.WriteLine($"Module {moduleDescriptor.ModuleType.Name} is not configurable and will not be executed.");
                        return Task.CompletedTask;
                    }

                    module.Configure(Configurator);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing module {moduleDescriptor.GetType().Name}");
                // TODO: Handle module execution errors (e.g., log, fallback)
            }

            return Task.CompletedTask;
        });
        await Task.WhenAll(tasks);

        using (_moduleRegistrationLock.EnterScope())
        {
            Configurator.Register();

            // 3. Sort MiddlewareDescriptors
            Debug.WriteLine("MiddlewareDescriptors:");
            foreach (var d in _middlewareDescriptors.OrderBy(d => d.Priority))
            {
                Debug.WriteLine($"Middleware Id: {d.Id} Index: {_middlewareDescriptors.ToList().IndexOf(d)} Priority: {d.Priority} Name: {d.MiddlewareType.Name}");
            }

            var sortedDescriptors = _middlewareDescriptors.OrderBy(d => d.Priority);
            var configurator = _innerBuilder.ApplicationServices.GetRequiredService<IMiddlewareConfigurator>();

            // 4. Build the Pipeline
            foreach (var descriptor in sortedDescriptors)
            {
                Configurator.Register(descriptor, default);
            }
        }

        // 5. Build the Application
        return _innerBuilder.Build();
    }

    /// <inheritdoc/>
    public RequestDelegate Build() => Build(null);

    internal RequestDelegate Build(Action<MiddlewareDescriptor, HttpContext>? register = null, int? priority = int.MaxValue)
    {
        using (_moduleRegistrationLock.EnterScope())
        {
            _sortedModules.AddRange(_registeredModules.OrderBy(m => m.Priority)
                                                      .ThenBy(m => _registeredModules.IndexOf(m)));

            var configurator = _innerBuilder.ApplicationServices.GetRequiredService<IMiddlewareConfigurator>();

            foreach (var moduleDescriptor in _sortedModules)
            {
                if (moduleDescriptor.Instance is IConfigurableModule<IMiddlewareConfigurator> configurableModule)
                {
                    configurableModule.Configure(Configurator);
                }
                else
                {
                    Debug.WriteLine($"Module {moduleDescriptor.ModuleType.Name} is not configurable and will not be executed.");
                }
            }

            foreach (var descriptor in _middlewareDescriptors.OrderBy(d => d.Priority))
            {
                descriptor.Priority = Math.Min(descriptor.Priority, priority ?? int.MaxValue);
                Configurator.Register(descriptor, register);
            }

            Debug.WriteLine("About to call _innerBuilder.Build().");
            //Configurator.Register(register);

            var result = _innerBuilder.Build();
            Debug.WriteLine("Build method called.");
            return result;
        }
    }

    /// <inheritdoc/>
    public IApplicationBuilder New() => new ApplicationBuilderWrapper(_innerBuilder.New(), _logger, Configurator);

    internal void UseDescriptor(MiddlewareDescriptor descriptor, Action<MiddlewareDescriptor, HttpContext, RequestDelegate> action, int? priority)
    {
        descriptor.Priority = descriptor.Priority == DefaultValues.Priority ? priority ?? DefaultValues.Priority : descriptor.Priority;

        if (!_middlewareDescriptors.Contains(descriptor))
        {
            _middlewareDescriptors.Add(descriptor);
        }

        if (descriptor.IsEnabled)
        {
            var request = async (HttpContext context, RequestDelegate next) =>
            {
                Debug.WriteLine($"Beginning Execution of Middleware {descriptor.Id}: {descriptor.MiddlewareType.Name} Priority: {descriptor.Priority}");
                if (descriptor.Timeout.HasValue)
                {
                    var timeoutToken = new CancellationTokenSource(descriptor.Timeout.Value).Token;
                    await Task.Run(() => action(descriptor, context, next), timeoutToken);
                }
                else
                {
                    action(descriptor, context, next);
                }

                Debug.WriteLine($"Finished Executing Middleware {descriptor.Id}: {descriptor.MiddlewareType.Name} Priority: {descriptor.Priority}");
            };

            _innerBuilder.Use(request);
        }
    }
}

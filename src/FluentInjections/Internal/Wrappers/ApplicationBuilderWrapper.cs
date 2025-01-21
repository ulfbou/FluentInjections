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

namespace FluentInjections.Internal.Wrappers;

public class ApplicationBuilderWrapper : IApplicationBuilder
{
    protected readonly IApplicationBuilder _innerBuilder;
    private readonly NetCoreMiddlewareConfigurator _configurator;
    protected readonly ILogger<ApplicationBuilderWrapper> _logger;
    protected readonly ConcurrentBag<MiddlewareDescriptor> _middlewareDescriptors = new();
    protected readonly Lock _moduleRegistrationLock = new();
    protected readonly List<ModuleDescriptor> _registeredModules = new();
    protected readonly ConcurrentDictionary<Type, Type> _moduleRegistrations = new();
    protected readonly List<ModuleDescriptor> _sortedModules = new();
    protected int _currentModulePriority;

    /// <inheritdoc/>
    public IServiceProvider ApplicationServices { get => _innerBuilder.ApplicationServices; set => _innerBuilder.ApplicationServices = value; }

    /// <inheritdoc/>
    public IFeatureCollection ServerFeatures => _innerBuilder.ServerFeatures;

    /// <inheritdoc/>
    public IDictionary<string, object?> Properties => _innerBuilder.Properties;

    public ApplicationBuilderWrapper(IApplicationBuilder innerBuilder, IMiddlewareConfigurator configurator, ILogger<ApplicationBuilderWrapper> logger)
    {
        _innerBuilder = innerBuilder ?? throw new ArgumentNullException(nameof(innerBuilder));
        _configurator = configurator as NetCoreMiddlewareConfigurator ?? throw new ArgumentNullException(nameof(configurator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IApplicationBuilder Use(Type middlewareType)
    {
        using (_moduleRegistrationLock.EnterScope())
        {
            _middlewareDescriptors.Add(new MiddlewareDescriptor(middlewareType, _configurator)
            {
                Priority = _currentModulePriority,
            });
            return this;
        }
    }

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        using (_moduleRegistrationLock.EnterScope())
        {
            _middlewareDescriptors.Add(new MiddlewareDescriptor(middleware.GetType(), _configurator)
            {
                Instance = middleware,
                Priority = _currentModulePriority,
            });

            return this;
        }
    }

    public IApplicationBuilder UseMiddleware<TMiddleware>() where TMiddleware : IMiddleware
    {
        using (_moduleRegistrationLock.EnterScope())
        {
            var modulePriority = _currentModulePriority;
            _middlewareDescriptors.Add(new MiddlewareDescriptor(typeof(TMiddleware), _configurator)
            {
                Priority = modulePriority,
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
        await Task.WhenAll(_sortedModules.Select(async moduleDescriptor =>
        {
            using var scope = _innerBuilder.ApplicationServices.CreateScope();

            try
            {
                using (_moduleRegistrationLock.EnterScope())
                {
                    _currentModulePriority = moduleDescriptor.Priority;

                    if (moduleDescriptor.Instance is IConfigurableModule<IMiddlewareConfigurator> configurableModule)
                    {
                        configurableModule.Configure(_configurator);
                    }
                    else if (moduleDescriptor.Instance is IMiddlewareModule module)
                    {
                        module.Configure(_configurator);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing module {moduleDescriptor.GetType().Name}");
                // TODO: Handle module execution errors (e.g., log, fallback)
            }
        }));

        using (_moduleRegistrationLock.EnterScope())
        {
            // 3. Sort MiddlewareDescriptors
            var sortedDescriptors = _middlewareDescriptors.OrderBy(d => d.Priority);
            var configurator = _innerBuilder.ApplicationServices.GetRequiredService<IMiddlewareConfigurator>();

            // 4. Build the Pipeline
            foreach (var descriptor in sortedDescriptors)
            {
                _configurator.Register(descriptor, default);
            }
        }

        // 5. Build the Application
        return _innerBuilder.Build();
    }

    /// <inheritdoc/>
    public RequestDelegate Build() => _innerBuilder.Build();

    /// <inheritdoc/>
    public IApplicationBuilder New() => _innerBuilder.New();
}

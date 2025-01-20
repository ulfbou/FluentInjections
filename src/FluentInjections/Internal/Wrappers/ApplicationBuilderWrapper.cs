// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace FluentInjections.Internal.Wrappers;

using FluentInjections.Internal.Configurators;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static FluentInjections.Internal.Wrappers.ApplicationBuilderWrapper;

public class ApplicationBuilderWrapper : IApplicationBuilder
{
    private readonly IApplicationBuilder _innerBuilder;
    private readonly NetCoreMiddlewareConfigurator _configurator;
    private readonly ILogger<ApplicationBuilderWrapper> _logger;
    private readonly ConcurrentBag<MiddlewareDescriptor> _middlewareDescriptors = new();
    private readonly SemaphoreSlim _moduleRegistrationLock = new(1);
    private readonly List<ModuleDescriptor> _registeredModules = new();
    private readonly ConcurrentDictionary<Type, Type> _moduleRegistrations = new();
    private readonly List<ModuleDescriptor> _sortedModules = new();
    private int _currentModulePriority;

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

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        try
        {
            using (var semaphore = _moduleRegistrationLock.DisposableWait())
            {
                _middlewareDescriptors.Add(new MiddlewareDescriptor(middleware.GetType(), _configurator)
                {
                    Instance = middleware,
                    Priority = _currentModulePriority,
                });

                return this;
            }
        }
        finally
        {
            _moduleRegistrationLock.Release();
        }
    }

    public IApplicationBuilder UseMiddleware<TMiddleware>() where TMiddleware : IMiddleware
    {
        _moduleRegistrationLock.DisposableWait();

        var modulePriority = _currentModulePriority;
        _middlewareDescriptors.Add(new MiddlewareDescriptor(typeof(TMiddleware), _configurator)
        {
            Priority = modulePriority,
        });

        return this;
    }

    public IApplicationBuilder Use(Action<IApplicationBuilder> configure)
    {
        try
        {
            _moduleRegistrationLock.DisposableWait();
            configure(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring application builder");
            // TODO: Handle configuration errors (e.g., log, fallback)
        }

        return this;
    }

    public void RegisterModule<TModule>(TModule module) where TModule : Module<IMiddlewareConfigurator>
    {
        _moduleRegistrationLock.DisposableWait();
        _registeredModules.Add(new ModuleDescriptor
        {
            ModuleType = typeof(TModule),
            Instance = module
        });
    }

    public async Task<RequestDelegate> BuildAsync()
    {
        // 1. Sort Modules
        using (var semaphore = _moduleRegistrationLock.DisposableWait())
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
                _moduleRegistrationLock.DisposableWait();
                _currentModulePriority = moduleDescriptor.Priority;
                var module = moduleDescriptor.Instance as Module<IMiddlewareConfigurator>;
                module?.Configure(_configurator);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing module {moduleDescriptor.GetType().Name}");
                // TODO: Handle module execution errors (e.g., log, fallback)
            }
            finally
            {
                _moduleRegistrationLock.Release();
            }
        }));

        _moduleRegistrationLock.DisposableWait();

        // 3. Sort MiddlewareDescriptors
        var sortedDescriptors = _middlewareDescriptors.OrderBy(d => d.Priority);
        var configurator = _innerBuilder.ApplicationServices.GetRequiredService<IMiddlewareConfigurator>();

        // 4. Build the Pipeline
        foreach (var descriptor in sortedDescriptors)
        {
            _configurator.Register(descriptor, default);
        }

        // 5. Build the Application
        return _innerBuilder.Build();
    }

    /// <inheritdoc/>
    public RequestDelegate Build() => _innerBuilder.Build();

    /// <inheritdoc/>
    public IApplicationBuilder New() => _innerBuilder.New();
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.ServiceProvider;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentInjections.Internal.Configurators;

internal class NetCoreServiceConfigurator : ServiceConfigurator, IServiceConfigurator
{
    protected readonly IDictionary<string, ServiceDescriptor> _keyedServiceDescriptors = new Dictionary<string, ServiceDescriptor>();
    protected readonly IServiceCollection _services;
    protected IServiceProvider? _provider;

    public override IServiceCollection Services => _services;
    public IServiceProvider Provider
    {
        get => _provider ?? throw new InvalidOperationException("The service provider has not been initialized.");
        protected set => _provider = value;
    }

    public NetCoreServiceConfigurator(IServiceCollection services, ILogger<NetCoreServiceConfigurator> logger) : base(logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    protected override void Register(ServiceBindingDescriptor bindingDescriptor)
    {
        _services.Register(bindingDescriptor);
    }

    internal virtual NetCoreServiceProvider BuildServiceProvider(IServiceCollection? services = null)
    {
        var innerProvider = (services ?? _services).BuildServiceProvider();
        return new NetCoreServiceProvider(innerProvider, NetCoreNamedExtensions.NamedServices);
    }

    internal IDictionary<string, ServiceDescriptor> GetKeyedServiceDescriptors() => _keyedServiceDescriptors;
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

using FluentInjections.Internal.Descriptors;
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
    private readonly IDictionary<string, ServiceDescriptor> _keyedServiceDescriptors = new Dictionary<string, ServiceDescriptor>();
    private readonly IServiceCollection _services;
    internal IServiceCollection DependencyBuilder => _services;

    public NetCoreServiceConfigurator(IServiceCollection services, ILogger<NetCoreServiceConfigurator> logger) : base(logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    protected override void Register(ServiceBindingDescriptor bindingDescriptor)
    {
        _services.Register(bindingDescriptor);
    }

    internal IServiceProvider BuildServiceProvider()
    {
        var serviceProvider = _services.BuildServiceProvider();
        return new NetCoreServiceProvider(serviceProvider, _keyedServiceDescriptors);
    }

    internal IDictionary<string, ServiceDescriptor> GetKeyedServiceDescriptors() => _keyedServiceDescriptors;
}

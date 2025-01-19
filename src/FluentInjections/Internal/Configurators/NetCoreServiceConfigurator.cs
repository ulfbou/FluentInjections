// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;
using FluentInjections.Internal.ServiceProvider;
using FluentInjections.Internal.Utils;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using ServiceDescriptor = FluentInjections.Internal.Descriptors.ServiceDescriptor;

namespace FluentInjections.Internal.Configurators;

internal class NetCoreServiceConfigurator : ServiceConfigurator, IServiceConfigurator
{
    protected readonly IDictionary<string, ServiceDescriptor> _keyedServiceDescriptors = new Dictionary<string, ServiceDescriptor>();
    protected readonly IServiceCollection _services;
    protected IConfigurationManager? _configuration;
    protected ILoggingBuilder? _logging;
    protected IMetricsBuilder? _metrics;
    protected IServiceProvider? _provider;

    public override IServiceCollection Services => _services;
    public override IConfigurationManager Configuration => _configuration ?? throw new InvalidOperationException("The configuration manager has not been initialized.");
    public override ILoggingBuilder Logging => _logging ?? throw new InvalidOperationException("The logging builder has not been initialized.");
    public override IMetricsBuilder Metrics => _metrics ?? throw new InvalidOperationException("The metrics builder has not been initialized.");

    public override IServiceProvider Provider
    {
        get => _provider ?? throw new InvalidOperationException("The service provider has not been initialized.");
        set => _provider = value;
    }

    public NetCoreServiceConfigurator(IServiceCollection services, ILogger<NetCoreServiceConfigurator> logger) : base(logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public NetCoreServiceConfigurator(IFluentInjectionsBuilder builder) : this(builder.Services, LoggerUtility.CreateLogger<NetCoreServiceConfigurator>())
    {
        _configuration = builder.Configuration;
        _logging = builder.Logging;
        _metrics = builder.Metrics;
    }

    protected override void Register(ServiceDescriptor bindingDescriptor)
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

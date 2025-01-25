// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Internal.Wrappers;

internal class WebApplicationBuilderWrapper : IHostApplicationBuilder
{
    private WebApplicationBuilder _builder;
    private ILogger<WebApplicationBuilderWrapper> _logger;

    public WebApplicationBuilderWrapper(WebApplicationBuilder builder, ILogger<WebApplicationBuilderWrapper> logger)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IConfigurationManager Configuration => ((IHostApplicationBuilder)_builder).Configuration;

    public IHostEnvironment Environment => ((IHostApplicationBuilder)_builder).Environment;

    public ILoggingBuilder Logging => _builder.Logging;

    public IMetricsBuilder Metrics => _builder.Metrics;

    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_builder).Properties;

    public IServiceCollection Services => _builder.Services;

    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull => ((IHostApplicationBuilder)_builder).ConfigureContainer(factory, configure);
}
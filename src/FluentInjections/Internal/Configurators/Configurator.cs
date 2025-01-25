// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Managers;
using FluentInjections.Internal.Utils;
using FluentInjections.Internal.Wrappers;
using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluentInjections.Internal.Configurators;

/// <summary>
/// Represents a class that creates and manages the host builder,  and service manager.
public sealed class Configurator
{
    private static IHostApplicationBuilder? _hostBuilder;
    private static ServiceManager? _serviceManager;
    private static WebApplicationBuilder? _innerHostBuilder;

    public static WebApplication InnerApp { get; private set; }
    public static ApplicationBuilderWrapper App { get; private set; }

    internal static ServiceManager ServiceManager
    {
        get
        {
            if (_serviceManager is null)
            {
                var hostBuilder = HostBuilder;
                _serviceManager = new ServiceManager(hostBuilder.Services);
            }

            return _serviceManager;
        }
    }

    internal static IHostApplicationBuilder HostBuilder
    {
        get
        {
            if (_hostBuilder is null)
            {
                _hostBuilder = CreateWebApplicationBuilder();
            }

            return _hostBuilder;
        }
    }

    internal static WebApplicationBuilder InnerHostBuilder
    {
        get
        {
            if (_innerHostBuilder is null)
            {
                _innerHostBuilder = WebApplication.CreateBuilder();
            }

            return _innerHostBuilder;
        }
    }

    public static IHostApplicationBuilder CreateWebApplicationBuilder()
    {
        if (HostBuilder is not null)
        {
            return HostBuilder;
        }

        return new WebApplicationBuilderWrapper(InnerHostBuilder, LoggerUtility.CreateLogger<WebApplicationBuilderWrapper>());
    }

    internal static ApplicationBuilderWrapper CreateApplicationBuilder(WebApplicationBuilderWrapper hostBuilder)
    {
        Guard.NotNull(hostBuilder, nameof(hostBuilder));

        InnerApp = InnerHostBuilder.Build();
        App = new ApplicationBuilderWrapper(InnerApp, LoggerUtility.CreateLogger<ApplicationBuilderWrapper>());

        return App;
    }
}

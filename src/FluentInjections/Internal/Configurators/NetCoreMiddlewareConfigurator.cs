// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using FluentInjections.Validation;
using FluentInjections.Internal.Wrappers;
using System.Text;
using FluentInjections.Internal.Constants;

namespace FluentInjections.Internal.Configurators;

internal class NetCoreMiddlewareConfigurator
    : MiddlewareConfigurator<IApplicationBuilder, IMiddlewareBinding>,
    IMiddlewareConfigurator<IApplicationBuilder, IMiddlewareBinding>, IMiddlewareConfigurator, IConfigurator<IMiddlewareBinding>
{
    internal NetCoreMiddlewareConfigurator(IApplicationBuilder builder, IServiceProvider provider, ILogger<NetCoreMiddlewareConfigurator> logger, NetCoreMiddlewareConfigurator? configurator = null)
        : base(builder, provider, logger)
    {
        if (configurator is not null)
        {
            _registeredMiddlewareTypes.AddRange(configurator._registeredMiddlewareTypes);
            _bindings.AddRange(configurator._bindings);
            _conflictResolution = configurator._conflictResolution;
            _middleware = configurator._middleware;
            _middlewareType = configurator._middlewareType;
        }
    }

    protected override void Register(MiddlewareDescriptor descriptor, int? priority = int.MaxValue)
    {
        Register(descriptor, null);
    }

    internal IReadOnlyList<Type> GetRegisteredMiddlewareTypes() => _registeredMiddlewareTypes;

    internal override IApplicationBuilder GetApplication()
    {
        if (_dependencyBuilder is not IApplicationBuilder builder)
        {
            throw new InvalidOperationException("The provided builder is not supported.");
        }

        if (_dependencyBuilder is not ApplicationBuilderWrapper wrapper)
        {
            return builder;
        }

        return builder;
    }
}

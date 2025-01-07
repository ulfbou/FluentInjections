// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using FluentInjections.Validation;

namespace FluentInjections.Internal.Configurators;

internal class NetCoreMiddlewareConfigurator
    : MiddlewareConfigurator<IApplicationBuilder, IMiddlewareBinding>,
    IMiddlewareConfigurator<IApplicationBuilder, IMiddlewareBinding>, IMiddlewareConfigurator, IConfigurator<IMiddlewareBinding>
{
    internal NetCoreMiddlewareConfigurator(IApplicationBuilder builder, ILogger<NetCoreMiddlewareConfigurator> logger)
        : base(builder, logger)
    { }

    protected override void Register(MiddlewareBindingDescriptor descriptor)
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

        return builder;
    }
}

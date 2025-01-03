// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using FluentInjections.Validation;

namespace FluentInjections.Internal.Configurators;

internal class AutofacMiddlewareConfigurator : MiddlewareConfigurator<ContainerBuilder, IMiddlewareBinding>, IMiddlewareConfigurator<ContainerBuilder, IMiddlewareBinding>
{
    private IContainer? _container;

    public AutofacMiddlewareConfigurator(ContainerBuilder container, ILogger<AutofacMiddlewareConfigurator> logger)
        : base(container, logger)
    {
    }

    internal IContainer? Container => _container;

    protected override void Register(MiddlewareBindingDescriptor descriptor)
    {
        Register(descriptor, null);
    }

    internal override IApplicationBuilder GetApplication()
    {
        if (_dependencyBuilder is not ContainerBuilder builder)
        {
            throw new InvalidOperationException("The provided builder is not supported.");
        }

        _container ??= builder.Build();
        return _container.Resolve<IApplicationBuilder>();
    }
}

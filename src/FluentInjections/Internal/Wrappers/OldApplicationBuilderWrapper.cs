// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace FluentInjections.Internal.Wrappers;

using Microsoft.AspNetCore.Builder;

using System;
using System.Collections.Generic;
using System.Threading;

public class OldApplicationBuilderWrapper : IApplicationBuilder
{
    private readonly IDictionary<string, object?> _properties = new Dictionary<string, object?>();
    private readonly IApplicationBuilder _applicationBuilder;
    private readonly Stack<(int, Module)> _modules = new();

    /// <inheritdoc/>
    public IServiceProvider ApplicationServices
    {
        get => _applicationBuilder.ApplicationServices;
        set
        {
            _applicationBuilder.ApplicationServices = value;
        }
    }

    /// <inheritdoc/>
    public IFeatureCollection ServerFeatures => _applicationBuilder.ServerFeatures;

    /// <inheritdoc/>
    public IDictionary<string, object?> Properties => _properties;

    /// <summary>
    /// A lock to ensure thread-safe behavior internally. 
    /// </summary>
    internal Lock Lock { get; set; }

    public OldApplicationBuilderWrapper(IApplicationBuilder applicationBuilder)
    {
        _applicationBuilder = applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));
        Lock = new Lock();
    }

    /// <inheritdoc/>
    public RequestDelegate Build() => throw new NotImplementedException();

    /// <inheritdoc/>
    public IApplicationBuilder New() => throw new NotImplementedException();

    /// <inheritdoc/>
    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) => throw new NotImplementedException();

    public Lock GetPriority<TModule>(int priority, TModule module) where TModule : IConfigurableModule
    {
        return new Lock();
    }

}

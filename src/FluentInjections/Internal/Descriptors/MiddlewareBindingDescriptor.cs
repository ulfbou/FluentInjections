// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Constants;

using FluentInjections.Validation;

using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentInjections.Internal.Descriptors;

/// <summary>
/// Represents a middleware binding descriptor that provides methods to configure middleware within the application.
/// </summary>
/// <remarks>
/// This class is used to specify middleware binding configurations.
/// </remarks>
public class MiddlewareBindingDescriptor
{
    private readonly object _lock = new();

    public Type MiddlewareType { get; }
    public object? Instance { get; set; }
    public string? Name { get; set; }
    public int Priority { get; set; } = DefaultValues.Priority;
    public string Group { get; set; } = DefaultValues.Group;
    public string? RequiredEnvironment { get; set; }
    public Func<IServiceProvider, object>? ExecutionPolicyFactory { get; set; }
    public Action<object>? ExecutionPolicyConfiguration { get; set; }

    public Func<HttpContext, Task>? Fallback { get; set; }
    public object? Options { get; set; }
    public Type? OptionsType { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<Type> Dependencies { get; set; } = new();
    public List<Type> PrecedingMiddleware { get; set; } = new();
    public List<Type> FollowingMiddleware { get; set; } = new();
    public TimeSpan? Timeout { get; set; }
    public Func<HttpContext, Exception, Task>? ErrorHandler { get; set; }
    public string? Tag { get; set; }
    public Func<bool>? Condition { get; set; }
    public bool IsEnabled => Condition?.Invoke() ?? true;

    public IMiddlewareConfigurator MiddlewareConfigurator { get; }

    internal MiddlewareBindingDescriptor(Type middlewareType, IMiddlewareConfigurator middlewareConfigurator)
    {
        MiddlewareType = middlewareType ?? throw new ArgumentNullException(nameof(middlewareType));
        MiddlewareConfigurator = middlewareConfigurator ?? throw new ArgumentNullException(nameof(middlewareConfigurator));
    }

    /// <summary>
    /// Adds a dependency to the middleware binding descriptor.
    /// </summary>
    /// <param name="dependency">The dependency type to add.</param>
    public MiddlewareBindingDescriptor AddDependency(Type dependency)
    {
        Guard.NotNull(dependency, nameof(dependency));

        lock (_lock)
        {
            Dependencies.Add(dependency);
        }
        return this;
    }

    /// <summary>
    /// Adds a preceding middleware to the middleware binding descriptor.
    /// </summary>
    /// <param name="precedingMiddleware">The preceding middleware type to add.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public MiddlewareBindingDescriptor AddMetadata(string key, object value)
    {
        Guard.NotNull(key, nameof(key));
        Guard.NotNull(value, nameof(value));

        lock (_lock)
        {
            Metadata[key] = value;
        }

        return this;
    }

    /// <summary>
    /// Adds a succeeding middleware to the middleware binding descriptor.
    /// </summary>
    /// <param name="succedingMiddleware">The succeding middleware type to add.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public MiddlewareBindingDescriptor AddPrecedingMiddleware(Type succedingMiddleware)
    {
        Guard.NotNull(succedingMiddleware, nameof(succedingMiddleware));

        lock (_lock)
        {
            PrecedingMiddleware.Add(succedingMiddleware);
        }

        return this;
    }

    /// <summary>
    /// Validates the middleware binding descriptor.
    /// </summary>
    public void Validate()
    {
        Guard.NotNull(MiddlewareType, nameof(MiddlewareType));

        if (Dependencies.Distinct().Count() != Dependencies.Count)
        {
            throw new InvalidOperationException("Dependencies contain duplicates.");
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        Guard.NotNull(obj, nameof(obj));

        return obj is MiddlewareBindingDescriptor other &&
            MiddlewareType == other.MiddlewareType &&
            Priority == other.Priority &&
            Group == other.Group;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(MiddlewareType, Priority, Group);
}

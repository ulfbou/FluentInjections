// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Constants;

using FluentInjections.Validation;

using Microsoft.AspNetCore.Http;

namespace FluentInjections.Internal.Descriptors;

/// <summary>
/// Represents a middleware binding descriptor that provides methods to configure middleware within the application.
/// </summary>
/// <remarks>
/// This class is used to specify middleware binding configurations.
/// </remarks>
public class MiddlewareDescriptor
{
    private static int _currentId = 0;
    private readonly object _lock = new();
    public int Id { get; } = _currentId++;
    virtual public Type MiddlewareType { get; set; }
    public Func<IServiceProvider, RequestDelegate>? MiddlewareFactory { get; set; }
    public object? Instance { get; set; }
    public string? Name { get; set; }
    public int Priority { get; set; } = DefaultValues.Priority;
    public string Group { get; set; } = DefaultValues.Group;
    public string? RequiredEnvironment { get; set; }
    public Func<IServiceProvider, object>? ExecutionPolicyFactory { get; set; }
    public Action<object>? ExecutionPolicyConfiguration { get; set; }

    public Func<HttpContext, Task>? Fallback { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<Type> Dependencies { get; set; } = new();
    public List<Type> PrecedingMiddleware { get; set; } = new();
    public List<Type> FollowingMiddleware { get; set; } = new();
    public TimeSpan? Timeout { get; set; }
    public Func<Exception, Task>? ErrorHandler { get; set; }
    public string? Tag { get; set; }
    public Func<bool>? Condition { get; set; }
    public bool IsEnabled => Condition?.Invoke() ?? true;

    public IMiddlewareConfigurator MiddlewareConfigurator { get; }

    internal MiddlewareDescriptor(Type middlewareType, IMiddlewareConfigurator middlewareConfigurator)
    {
        MiddlewareType = middlewareType ?? throw new ArgumentNullException(nameof(middlewareType));

        MiddlewareConfigurator = middlewareConfigurator ?? throw new ArgumentNullException(nameof(middlewareConfigurator));
    }

    /// <summary>
    /// Sets the instance of the middleware binding descriptor.
    /// </summary>
    /// <param name="instance">The instance to set.</param>
    public void SetInstance(object instance)
    {
        lock (_lock)
        {
            Instance = instance;
        }
    }

    /// <summary>
    /// Adds a dependency to the middleware binding descriptor.
    /// </summary>
    /// <param name="dependency">The dependency type to add.</param>
    public MiddlewareDescriptor AddDependency(Type dependency)
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
    public MiddlewareDescriptor AddMetadata(string key, object value)
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
    public MiddlewareDescriptor AddPrecedingMiddleware(Type succedingMiddleware)
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
}

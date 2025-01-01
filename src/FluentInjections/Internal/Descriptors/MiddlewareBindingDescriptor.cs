// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Constants;

namespace FluentInjections.Internal.Descriptors;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MiddlewareBindingDescriptor
{
    private readonly object _lock = new();

    public Type MiddlewareType { get; }
    public object? Instance { get; set; }
    public int Priority { get; set; } = DefaultValues.Priority;
    public string Group { get; set; } = DefaultValues.Group;
    public string? RequiredEnvironment { get; set; }
    public object? ExecutionPolicy { get; set; }
    public Func<object, Task>? Fallback { get; set; }
    public object? Options { get; set; }
    public Type? OptionsType { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<Type> Dependencies { get; set; } = new();
    public List<Type> PrecedingMiddleware { get; set; } = new();
    public List<Type> FollowingMiddleware { get; set; } = new();
    public TimeSpan? Timeout { get; set; }
    public Func<Exception, Task>? ErrorHandler { get; set; }
    public string? Tag { get; set; }
    public Func<bool>? Condition { get; set; }
    public bool IsEnabled => Condition?.Invoke() ?? true;

    internal MiddlewareBindingDescriptor(Type middlewareType)
    {
        MiddlewareType = middlewareType ?? throw new ArgumentNullException(nameof(middlewareType));
    }

    public MiddlewareBindingDescriptor AddDependency(Type dependency)
    {
        if (dependency == null) throw new ArgumentNullException(nameof(dependency));
        lock (_lock)
        {
            Dependencies.Add(dependency);
        }
        return this;
    }

    public MiddlewareBindingDescriptor AddMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        if (value == null) throw new ArgumentNullException(nameof(value));
        lock (_lock)
        {
            Metadata[key] = value;
        }
        return this;
    }

    public void Validate()
    {
        if (MiddlewareType == null) throw new InvalidOperationException("MiddlewareType must be set.");
        if (Dependencies.Distinct().Count() != Dependencies.Count)
            throw new InvalidOperationException("Dependencies contain duplicates.");
    }

    public override bool Equals(object? obj) =>
        obj is MiddlewareBindingDescriptor other &&
        MiddlewareType == other.MiddlewareType &&
        Priority == other.Priority &&
        Group == other.Group;

    public override int GetHashCode() => HashCode.Combine(MiddlewareType, Priority, Group);
}

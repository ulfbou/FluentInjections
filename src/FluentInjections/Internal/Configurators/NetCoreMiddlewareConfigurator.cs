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

internal class NetCoreMiddlewareConfigurator<TDependencyBuilder>
    : MiddlewareConfigurator<TDependencyBuilder, IMiddlewareBinding>,
    IMiddlewareConfigurator<ServiceCollection, IMiddlewareBinding>, IMiddlewareConfigurator, IConfigurator<IMiddlewareBinding>
    where TDependencyBuilder : class
{
    internal NetCoreMiddlewareConfigurator(TDependencyBuilder builder, ILogger<NetCoreMiddlewareConfigurator<TDependencyBuilder>> logger)
        : base(builder, logger)
    { }

    protected override void Register(MiddlewareBindingDescriptor descriptor)
    {
        Register(descriptor, null);
    }

    internal IReadOnlyList<Type> GetRegisteredMiddlewareTypes() => _registeredMiddlewareTypes;

    #region Middleware Ordering
    private List<MiddlewareBindingDescriptor> OrderMiddlewareBindingDescriptors()
    {
        var graph = new Dictionary<MiddlewareBindingDescriptor, List<MiddlewareBindingDescriptor>>();

        // Initialize graph with all descriptors
        foreach (var descriptor in _descriptors)
        {
            graph[descriptor] = new List<MiddlewareBindingDescriptor>();
        }

        // Build the graph based on dependencies, preceding, and following relationships
        foreach (var descriptor in _descriptors)
        {
            if (descriptor.Dependencies != null)
            {
                foreach (var dependencyType in descriptor.Dependencies)
                {
                    var dependency = FindMiddlewareBindingDescriptor(dependencyType);
                    if (dependency != null)
                    {
                        graph[dependency].Add(descriptor); // Dependency must run before this middleware
                    }
                }
            }

            if (descriptor.PrecedingMiddleware != null)
            {
                foreach (var precedingType in descriptor.PrecedingMiddleware)
                {
                    var precedingMiddleware = FindMiddlewareBindingDescriptor(precedingType);
                    if (precedingMiddleware != null)
                    {
                        graph[precedingMiddleware].Add(descriptor); // Preceding middleware must run before this middleware
                    }
                }
            }

            if (descriptor.FollowingMiddleware != null)
            {
                foreach (var followingType in descriptor.FollowingMiddleware)
                {
                    var followingMiddleware = FindMiddlewareBindingDescriptor(followingType);
                    if (followingMiddleware != null)
                    {
                        graph[descriptor].Add(followingMiddleware); // This middleware must run before following middleware
                    }
                }
            }
        }

        // Perform topological sorting
        var sortedDescriptors = TopologicalSort(graph);

        // Apply secondary sorting by priority
        return sortedDescriptors
            .OrderBy(d => d.Priority)
            .ToList();
    }

    private MiddlewareBindingDescriptor? FindMiddlewareBindingDescriptor(Type middlewareType)
    {
        return _descriptors.FirstOrDefault(d => d.MiddlewareType == middlewareType);
    }

    private List<MiddlewareBindingDescriptor> TopologicalSort(Dictionary<MiddlewareBindingDescriptor, List<MiddlewareBindingDescriptor>> graph)
    {
        var sorted = new List<MiddlewareBindingDescriptor>();
        var visited = new HashSet<MiddlewareBindingDescriptor>();
        var visiting = new HashSet<MiddlewareBindingDescriptor>();

        foreach (var node in graph.Keys)
        {
            Visit(node, graph, sorted, visited, visiting);
        }

        return sorted;
    }

    private void Visit(
        MiddlewareBindingDescriptor node,
        Dictionary<MiddlewareBindingDescriptor, List<MiddlewareBindingDescriptor>> graph,
        List<MiddlewareBindingDescriptor> sorted,
        HashSet<MiddlewareBindingDescriptor> visited,
        HashSet<MiddlewareBindingDescriptor> visiting)
    {
        if (visited.Contains(node))
            return;

        if (visiting.Contains(node))
        {
            throw new InvalidOperationException($"Circular dependency detected with middleware: {node.MiddlewareType?.FullName}");
        }

        visiting.Add(node);

        foreach (var dependency in graph[node])
        {
            Visit(dependency, graph, sorted, visited, visiting);
        }

        visiting.Remove(node);
        visited.Add(node);
        sorted.Add(node);
    }
    #endregion

    internal override IApplicationBuilder GetApplication()
    {
        if (_dependencyBuilder is not IApplicationBuilder builder)
        {
            throw new InvalidOperationException("The provided builder is not supported.");
        }

        return builder;
    }
}

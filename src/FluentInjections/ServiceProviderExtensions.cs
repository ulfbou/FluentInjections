// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Get a named service from the service provider.
    /// </summary>
    /// <typeparam name="TService">The type of service to get.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The service instance.</returns>
    //public static TService? GetNamedService<TService>(this IServiceProvider provider, string name) where TService : notnull
    //{
    //    return provider.GetKeyedService<TService>(name);
    //}

    /// <summary>
    /// Get a required named service from the service provider.
    /// </summary>
    /// <typeparam name="TService">The type of service to get.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The service instance.</returns>
    //public static TService GetRequiredNamedService<TService>(this IServiceProvider provider, string name) where TService : notnull
    //{
    //    return provider.GetRequiredKeyedService<TService>(name);
    //}
}

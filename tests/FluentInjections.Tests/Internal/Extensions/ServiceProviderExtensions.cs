using FluentInjections.Internal.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Tests.Internal.Extensions;

internal static class ServiceProviderExtensions
{
    /// <summary>
    /// Gets a named service from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The service instance or null if not found.</returns>
    public static T? GetNamedService<T>(this IServiceProvider provider, string name) where T : class
    {
        var serviceCollection = provider.GetRequiredService<IServiceCollection>();
        var serviceDescriptor = serviceCollection.FirstOrDefault(
            descriptor => descriptor.ServiceType == typeof(T) &&
            descriptor is NamedServiceDescriptor named &&
            named.Name == name);

        return serviceDescriptor?.ImplementationInstance as T;
    }

    /// <summary>
    /// Gets a named service from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="name">The name of the service.</param>
    /// <param name="defaultValue">The default value to return if the service is not found.</param>
    /// <returns>The service instance or the default value if not found.</returns>
    public static T? GetNamedService<T>(this IServiceProvider provider, string name, T defaultValue) where T : class
    {
        var service = provider.GetNamedService<T>(name);
        return service ?? defaultValue;
    }

    /// <summary>
    /// Gets a required named service from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="name">The name of the service.</param>
    /// <returns>The service instance.</returns>
    public static T GetRequiredNamedService<T>(this IServiceProvider provider, string name) where T : class
    {
        var service = provider.GetNamedService<T>(name);
        if (service is null)
        {
            throw new InvalidOperationException($"No service of type {typeof(T).Name} with the name '{name}' was found.");
        }

        return service;
    }
}
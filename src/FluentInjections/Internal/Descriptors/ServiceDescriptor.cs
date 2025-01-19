// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Internal.Configurators;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.Descriptors;

/// <summary>
/// Represents a service descriptor that provides information about a service within the application.
/// </summary>
public class ServiceDescriptor
{
    public Type BindingType { get; }
    public ServiceLifetime Lifetime { get; set; }
    public Type? ImplementationType { get; set; }
    public object? Instance { get; set; }
    public Func<IServiceProvider, object>? Factory { get; set; }
    public string? Name { get; set; }
    public Action<object>? Configure { get; set; }
    public Dictionary<string, object?> Metadata { get; set; } = new();
    public Dictionary<string, object?> Parameters { get; set; } = new();
    public Func<bool>? Condition { get; set; }
    public IServiceConfigurator ServiceConfigurator { get; }

    public ServiceDescriptor(Type bindingType, IServiceConfigurator serviceConfigurator)
    {
        BindingType = bindingType ?? throw new ArgumentNullException(nameof(bindingType));
        ServiceConfigurator = serviceConfigurator ?? throw new ArgumentNullException(nameof(serviceConfigurator));
        Lifetime = ServiceLifetime.Transient;
    }

    public bool IsEnabled => Condition?.Invoke() ?? true;

    /// <summary>
    /// Sets the lifetime of the service.
    /// </summary>
    /// <param name="lifetime">The lifetime of the service.</param>
    /// <returns>The service descriptor.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the lifetime is invalid.</exception>
    public ServiceDescriptor SetLifetime(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
        return this;
    }

    /// <summary>
    /// Adds metadata to the service descriptor.
    /// </summary>
    /// <param name="key">The key of the metadata to add.</param>
    /// <param name="value">The metadata to add.</param>
    /// <returns>The service descriptor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the key or value is <see langword="null"/>.</exception>
    public ServiceDescriptor AddMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        Metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
        return this;
    }

    /// <summary>
    /// Adds a parameter to the service descriptor.
    /// </summary>
    /// <param name="key">The key of the parameter to add.</param>
    /// <param name="value">The parameter to add.</param>
    /// <returns>The service descriptor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the key or value is <see langword="null"/>.</exception>
    public ServiceDescriptor AddParameter(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        Parameters[key] = value ?? throw new ArgumentNullException(nameof(value));
        return this;
    }

    /// <summary>
    /// Validates the service descriptor.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the service descriptor is invalid.</exception>
    public void Validate()
    {
        if (BindingType == null) throw new InvalidOperationException("BindingType must be set.");
        if (ImplementationType == null && Factory == null && Instance == null)
            throw new InvalidOperationException("Either ImplementationType, Factory, or Instance must be set.");
    }
}

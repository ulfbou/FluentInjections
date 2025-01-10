// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Extensions;
using FluentInjections.Internal.Descriptors;
using FluentInjections.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace FluentInjections.Internal.ServiceProvider;

public sealed class NetCoreServiceProvider :
    IServiceProvider,
    ISupportRequiredService,
    IKeyedServiceProvider,
    IServiceProviderIsService,
    IServiceProviderIsKeyedService,
    IDisposable,
    IAsyncDisposable
{
    private readonly IServiceProvider _provider;
    private readonly IDictionary<string, ServiceDescriptor> _keyedServiceDescriptors;
    private readonly IDictionary<string, Dictionary<Type, ServiceBindingDescriptor>> _namedServices;

    public NetCoreServiceProvider(IServiceProvider serviceProvider, IDictionary<string, ServiceDescriptor> keyedServiceDescriptors)
    {
        _provider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _keyedServiceDescriptors = keyedServiceDescriptors ?? throw new ArgumentNullException(nameof(keyedServiceDescriptors));
        _namedServices = new Dictionary<string, Dictionary<Type, ServiceBindingDescriptor>>();
    }

    public NetCoreServiceProvider(IServiceProvider serviceProvider, Dictionary<string, Dictionary<Type, ServiceBindingDescriptor>> namedServices)
    {
        _provider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _keyedServiceDescriptors = new Dictionary<string, ServiceDescriptor>();
        _namedServices = namedServices ?? throw new ArgumentNullException(nameof(namedServices));
    }

    /// <inheritdoc />
    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        Guard.NotNull(serviceType, nameof(serviceType));
        Guard.NotNull(serviceKey, nameof(serviceKey));
        Guard.IsType<string>(serviceKey!);

        lock (_namedServices)
        {
            string name = (serviceKey as string)!;

            if (_namedServices.TryGetValue(name!, out var services) && services.TryGetValue(serviceType, out var descriptor))
            {
                return GetServiceFromDescriptor(descriptor);
            }
        }

        return default;
    }

    /// <inheritdoc />
    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        return GetKeyedService(serviceType, serviceKey) ?? throw new InvalidOperationException($"Service type '{serviceType.Name}' with service key '{serviceKey}' not found.");
    }

    /// <inheritdoc />
    public object GetRequiredService(Type serviceType)
    {
        return ResolveService(serviceType) ?? throw new InvalidOperationException($"No service for type '{serviceType.Name}' has been registered.");
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        return ResolveService(serviceType);
    }

    private object? ResolveService(Type serviceType)
    {
        var service = _provider.GetService(serviceType);

        if (service is not null) return service;

        if (serviceType.IsGenericType && !serviceType.IsConstructedGenericType)
        {
            var genericTypeDefinition = serviceType.GetGenericTypeDefinition();
            var genericArguments = serviceType.GetGenericArguments();

            foreach (var descriptor in _provider.GetServices<ServiceDescriptor>())
            {
                if (descriptor.ImplementationType is null) continue;
                if (descriptor.ServiceType.IsGenericTypeDefinition && descriptor.ServiceType == genericTypeDefinition)
                {
                    if (descriptor.ImplementationType.TryMakeGenericType(genericArguments, out var closedType))
                    {
                        return _provider.GetService(closedType);
                    }
                }
            }
        }

        return null;
    }

    private object? GetServiceFromDescriptor(ServiceBindingDescriptor descriptor)
    {
        if (descriptor.Instance is not null)
        {
            descriptor.Configure?.Invoke(descriptor.Instance);
            return descriptor.Instance;
        }

        if (descriptor.Factory is not null)
        {
            var service = descriptor.Factory(_provider);
            descriptor.Configure?.Invoke(service);
            return service;
        }

        if (descriptor.ImplementationType is not null)
        {
            object? service;

            if (descriptor.Parameters.Any())
            {
                var parameters = descriptor.Parameters.Values.ToArray();
                try
                {
                    service = ActivatorUtilities.CreateInstance(_provider, descriptor.ImplementationType, parameters!);
                }
                catch
                {
                    return default;
                }
            }
            else
            {
                service = _provider.GetService(descriptor.ImplementationType);
            }

            if (service is not null)
            {
                descriptor.Configure?.Invoke(service);
                return service;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public bool IsKeyedService(Type serviceType, object? serviceKey)
    {
        Guard.NotNull(serviceType, nameof(serviceType));
        Guard.NotNull(serviceKey, nameof(serviceKey));
        Guard.IsType<string>(serviceKey!);

        lock (_namedServices)
        {
            string name = (serviceKey as string)!;

            if (_namedServices.TryGetValue(name!, out var services) && services.TryGetValue(serviceType, out _))
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc />
    public bool IsService(Type serviceType)
    {
        Guard.NotNull(serviceType, nameof(serviceType));
        return _provider.GetService(serviceType) != null;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (_provider is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }
        Dispose();
        return default;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

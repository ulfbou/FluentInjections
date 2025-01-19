using FluentInjections.Validation;

using Microsoft.AspNetCore.Http;

using System.Diagnostics;

namespace FluentInjections.Internal.Descriptors;

public class EndpointDescriptor : IEndpointDescriptor
{
    public string Pattern { get; set; } = string.Empty;
    public EndpointMethod Method { get; set; }
    public Type ServiceType { get; set; } = typeof(object);
    public string? RequestType { get; set; }
    public string? Name { get; set; }
    public string? Group { get; set; }
    public int Priority { get; set; }
    public string? Tag { get; set; }
    public TimeSpan? Timeout { get; set; }
    public bool RequireAuthorization { get; set; }
    public Type? ValidationFilterType { get; set; }
    public object? ValidationFilterInstance { get; set; }
    public Func<Exception, Task>? ErrorHandler { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public required Func<object, object, object, Task<IResult>> Handler { get; set; }

    /// <inheritdoc/>
    public bool TryAddMetadata<TMetadata>(string key, TMetadata metadata) where TMetadata : notnull
    {
        Guard.NotNullOrWhiteSpace(key, nameof(key));
        Guard.NotNull(metadata, nameof(metadata));

        if (Metadata.ContainsKey(key))
        {
            Debug.WriteLine($"Metadata '{metadata}' already exists in endpoint descriptor.");
            return false;
        }

        Metadata.TryAdd(key, metadata);
        Debug.WriteLine($"Added metadata '{metadata}' with key '{key}' to endpoint descriptor.");
        return true;
    }

    /// <inheritdoc/>
    public bool TryRemoveMetadata(string key)
    {
        Guard.NotNullOrWhiteSpace(key, nameof(key));

        if (!Metadata.ContainsKey(key))
        {
            Debug.WriteLine($"Metadata with key '{key}' does not exist in endpoint descriptor.");
            return false;
        }

        Metadata.Remove(key);
        Debug.WriteLine($"Removed metadata with key '{key}' from endpoint descriptor.");
        return true;
    }

    /// <inheritdoc/>
    public bool TryGetMetadata<TMetadata>(string key, out TMetadata metadata)
    {
        Guard.NotNullOrWhiteSpace(key, nameof(key));

        if (Metadata.ContainsKey(key) && !(Metadata[key] is TMetadata typedMetadata))
        {
            metadata = (TMetadata)Metadata[key];
            return true;
        }

        metadata = default!;
        return false;
    }

    /// <inheritdoc/>
    public void SetHandler<TService, TRequest>(Func<TService, TRequest, HttpContext, Task<IResult>> handler)
    {
        Handler = (service, request, context) => handler((TService)service, (TRequest)request, (HttpContext)context);
    }
}

using FluentInjections.Validation;

using Microsoft.AspNetCore.Http;

using System.Diagnostics;

namespace FluentInjections.Internal.Descriptors;

internal class EndpointDescriptor : IEndpointDescriptor
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

    public bool TryAddMetadata(string key, object value)
    {
        Guard.NotNullOrEmpty(key, nameof(key));
        Guard.NotNull(value, nameof(value));

        if (Metadata.ContainsKey(key))
        {
            return false;
        }

        Debug.WriteLine($"Added metadata key '{key}' with value '{value}' to endpoint descriptor.");
        Metadata[key] = value;
        return true;
    }

    public bool TryGetMetadata<T>(string key, out T metadata)
    {
        Guard.NotNullOrEmpty(key, nameof(key));

        if (Metadata.TryGetValue(key, out var value) && value is T typedMetadata)
        {
            Debug.WriteLine($"Retrieved metadata key '{key}' with value '{typedMetadata}' from endpoint descriptor.");
            metadata = typedMetadata;
            return true;
        }

        metadata = default!;
        return false;
    }
}

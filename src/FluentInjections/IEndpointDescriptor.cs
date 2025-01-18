// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

public interface IEndpointDescriptor
{
    Func<Exception, Task>? ErrorHandler { get; set; }
    string? Group { get; set; }
    Dictionary<string, object> Metadata { get; set; }
    EndpointMethod Method { get; set; }
    string? Name { get; set; }
    string Pattern { get; set; }
    int Priority { get; set; }
    bool RequireAuthorization { get; set; }
    Type ServiceType { get; set; }
    string? Tag { get; set; }
    TimeSpan? Timeout { get; set; }
    object? ValidationFilterInstance { get; set; }
    Type? ValidationFilterType { get; set; }

    /// <summary>
    /// Adds metadata to the endpoint descriptor.
    /// </summary>
    bool TryAddMetadata(string key, object value);

    /// <summary>
    /// Retrieves metadata from the endpoint descriptor.
    /// </summary>
    bool TryGetMetadata<T>(string key, out T metadata);
}

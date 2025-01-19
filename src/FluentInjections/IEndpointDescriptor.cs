// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace FluentInjections;

/// <summary>
/// Represents an endpoint descriptor that provides information about an endpoint.
/// </summary>
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
    /// <typeparam name="TMetadata">The type of metadata to add.</typeparam>
    /// <param name="key">The key of the metadata to add.</param>
    /// <param name="metadata">The metadata to add.</param>
    /// <returns><see langword="true"/> if the metadata was added; otherwise, <see langword="false"/>.</returns>
    bool TryAddMetadata<TMetadata>(string key, TMetadata metadata) where TMetadata : notnull;

    /// <summary>
    /// Tries to get metadata from the endpoint descriptor.
    /// </summary>
    /// <typeparam name="TMetadata">The type of metadata to get.</typeparam>
    /// <param name="metadata">The metadata to get.</param>
    /// <returns><see langword="true"/> if the metadata was found; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// If the metadata is found, it is returned in the <paramref name="metadata"/> parameter.
    /// </remarks>
    bool TryGetMetadata<TMetadata>(string key, out TMetadata metadata);

    /// <summary>
    /// Removes metadata from the endpoint descriptor.
    /// </summary>
    /// <param name="key">The key of the metadata to remove.</param>
    /// <returns><see langword="true"/> if the metadata was removed; otherwise, <see langword="false"/>.</returns>
    bool TryRemoveMetadata(string key);
}

public interface IEndpointDescriptor<TService, TRequest> : IEndpointDescriptor
{
    Func<object, object, HttpContext, Task<IResult>> Handler { get; set; }

    /// <summary>
    /// Sets the handler for the endpoint descriptor.
    /// </summary>
    /// <typeparam name="TService">The type of service to handle the request.</typeparam>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    /// <param name="handler">The handler to set.</param>
    /// <remarks>
    /// The handler is a function that takes a typed service, a typed request, and an <see cref="HttpContext"/> and returns a <see cref="Task{TResult}"/>.
    /// </remarks>
    void SetHandler(Func<TService, TRequest, HttpContext, Task<IResult>> handler);
}

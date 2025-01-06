// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Caching;

namespace FluentInjections.Caching;

/// <summary>
/// Represents a cache manager.
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// Gets the value associated with the specified key, or adds a new value if it does not exist.
    /// </summary>
    T GetOrSet<T>(string key, Func<T> valueFactory, CacheOptions options);

    /// <summary>
    /// Invalidates the cache entry with the specified key.
    /// </summary>
    void Invalidate(string key);

    /// <summary>
    /// Invalidates all cache entries with keys that start with the specified prefix.
    /// </summary>
    void InvalidateByPrefix(string prefix);
}

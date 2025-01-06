// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Caching;

public interface ICacheProvider
{
    /// <summary>
    /// Get a value from the cache. 
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The key to look up</param>
    /// <returns>The value if found, otherwise null</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Set a value in the cache. 
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The key to store the value under</param>
    /// <param name="value">The value to store</param>
    /// <param name="options">Optional. Options for storing the value</param>
    /// <remarks>
    /// If the key already exists, the value will be overwritten.
    /// If options are not provided, the default options will be used.
    /// </remarks>
    void Set<T>(string key, T value, CacheOptions? options = null);

    /// <summary>
    /// Tries to remove a value from the cache.
    /// </summary>
    /// <param name="key">The key to remove</param>
    /// <remarks>
    /// If the key does not exist, this method does nothing.
    /// </remarks>
    void TryRemove(string key);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns><see langword="true"/> if the key exists, otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// This method does not check if the value is null or not.
    /// </remarks>
    bool Contains(string key);

    /// <summary>
    /// Get all keys in the cache.
    /// </summary>
    /// <param name="prefix">The prefix to filter keys by</param>
    /// <returns>All keys in the cache.</returns>
    /// <remarks>
    /// If a prefix is provided, only keys that start with the prefix will be returned.
    /// </remarks>
    IEnumerable<string> GetKeys(string prefix = "");

    /// <summary>
    /// Get statistics about the cache.
    /// </summary>
    /// <returns>Statistics about the cache.</returns>
    CacheStats GetStats();
}

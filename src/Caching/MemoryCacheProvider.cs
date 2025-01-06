// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Core;
using FluentInjections.Validation;

using Microsoft.Extensions.Caching.Memory;

namespace FluentInjections.Caching;

/// <summary>
/// Represents a cache provider that uses <see cref="IMemoryCache"/> as the underlying storage.
/// </summary>
/// <remarks>
/// This implementation uses an <see cref="IMemoryCache"/> instance to store and retrieve cached values. 
/// It is thread-safe and can be used in multi-threaded environments. 
/// </remarks>
internal class MemoryCacheProvider : Disposable, ICacheProvider, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = new HashSet<string>();
    private readonly object _lock = new object();

    public MemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    public T? Get<T>(string key)
    {
        Guard.NotNullOrEmpty(key, nameof(key));
        EnsureNotDisposed();

        lock (_lock)
        {
            return _cache.TryGetValue(key, out T? value) ? value : default;
        }
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, CacheOptions? options = null)
    {
        Guard.NotNullOrEmpty(key, nameof(key));
        Guard.NotNull(options, nameof(options));
        EnsureNotDisposed();

        lock (_lock)
        {
            _keys.Add(key);

            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options?.AbsoluteExpiration ?? DefaultValues.AbsoluteExpiration,
                SlidingExpiration = options?.SlidingExpiration ?? DefaultValues.SlidingExpiration
            });
        }
    }

    /// <inheritdoc />
    public void TryRemove(string key)
    {
        Guard.NotNullOrEmpty(key, nameof(key));
        EnsureNotDisposed();

        lock (_lock)
        {
            _keys.Remove(key);
            _cache.Remove(key);
        }
    }

    /// <inheritdoc />
    public bool Contains(string key)
    {
        Guard.NotNullOrEmpty(key, nameof(key));
        EnsureNotDisposed();

        lock (_lock)
        {
            return _cache.TryGetValue(key, out _);
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> GetKeys(string prefix = "")
    {
        Guard.NotNull(prefix, nameof(prefix));
        EnsureNotDisposed();

        lock (_lock)
        {
            return _keys.Where(key => key.StartsWith(prefix)).ToList();
        }
    }

    /// <inheritdoc />
    public CacheStats GetStats()
    {
        EnsureNotDisposed();

        lock (_lock)
        {
            var stats = _cache.GetCurrentStatistics();

            if (stats is null)
            {
                return new CacheStats();
            }

            return new CacheStats
            {
                Hits = stats.TotalHits,
                Misses = stats.TotalMisses,
                MemoryUsage = stats.CurrentEstimatedSize,
                NumberOfEntries = stats.CurrentEntryCount
            };
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cache.Dispose();
        }
    }
}

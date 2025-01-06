// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Core;
using FluentInjections.Validation;

using Microsoft.Extensions.Caching.Memory;

namespace FluentInjections.Caching;

/// <summary>
/// Wraps the <see cref="MemoryCache"/> class to provide a more testable interface.
/// </summary>
internal class MemoryCacheWrapper : Disposable, IMemoryCache
{
    private MemoryCache _cache;

    public MemoryCacheWrapper(MemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    public ICacheEntry CreateEntry(object key)
    {
        Guard.NotNull(key, nameof(key));
        EnsureNotDisposed();
        return _cache.CreateEntry(key);
    }

    /// <inheritdoc />
    public void Remove(object key)
    {
        Guard.NotNull(key, nameof(key));
        EnsureNotDisposed();
        _cache.Remove(key);
    }

    /// <inheritdoc />
    public bool TryGetValue(object key, out object? value)
    {
        Guard.NotNull(key, nameof(key));
        return _cache.TryGetValue(key, out value);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cache?.Dispose();
            _cache = default!;
        }
    }
}

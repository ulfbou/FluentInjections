// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Core;
using FluentInjections.Validation;

namespace FluentInjections.Caching;

/// <summary>
/// Default implementation of <see cref="ICacheManager"/>.
/// </summary>
/// <remarks>
/// This implementation uses an <see cref="ICacheProvider"/> to store and retrieve cached values.
/// </remarks>
internal class DefaultCacheManager : Disposable, ICacheManager
{
    private readonly ICacheProvider _cacheProvider;

    public DefaultCacheManager(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
    }

    /// <inheritdoc />
    public T GetOrSet<T>(string key, Func<T> valueFactory, CacheOptions options)
    {
        Guard.NotNull(valueFactory, nameof(valueFactory));
        Guard.NotNull(options, nameof(options));
        EnsureNotDisposed();

        if (_cacheProvider.Contains(key))
        {
            return _cacheProvider.Get<T>(key)!;
        }

        var value = valueFactory();
        _cacheProvider.Set(key, value, options);
        return value;
    }

    /// <inheritdoc />
    public void Invalidate(string key)
    {
        Guard.NotNullOrEmpty(key, nameof(key));
        EnsureNotDisposed();
        _cacheProvider.TryRemove(key);
    }

    /// <inheritdoc />
    public void InvalidateByPrefix(string prefix)
    {
        Guard.NotNullOrEmpty(prefix, nameof(prefix));
        EnsureNotDisposed();

        foreach (var key in _cacheProvider.GetKeys(prefix))
        {
            _cacheProvider.TryRemove(key);
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            (_cacheProvider as IDisposable)?.Dispose();
        }
    }
}

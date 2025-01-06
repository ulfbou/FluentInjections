using Microsoft.Extensions.Caching.Memory;

namespace FluentInjections.Caching;

internal static class DependencyInjection
{
    private static readonly Lazy<ICacheManager> _cacheManager = new Lazy<ICacheManager>(() =>
    {
        // Create a default cache manager with reasonable defaults
        var memoryCache = new MemoryCacheWrapper(new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions()));
        var cacheProvider = new MemoryCacheProvider(memoryCache);
        return new DefaultCacheManager(cacheProvider);
    });

    public static ICacheManager DefaultCacheManager => _cacheManager.Value;
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Caching;

public sealed class CacheStats
{
    public long Hits { get; internal set; }
    public long Misses { get; internal set; }
    public long? MemoryUsage { get; internal set; }
    public long NumberOfEntries { get; internal set; }
}

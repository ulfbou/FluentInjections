// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Caching;

public class CacheOptions
{
    public TimeSpan? AbsoluteExpiration { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public CachePriority Priority { get; set; }
}

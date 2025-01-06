// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace FluentInjections.Caching;

internal class DefaultValues
{
    public static TimeSpan AbsoluteExpiration = TimeSpan.FromMinutes(5);
    public static TimeSpan SlidingExpiration = TimeSpan.FromMinutes(5);
}

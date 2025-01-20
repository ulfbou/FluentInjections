// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Internal.Extensions;

using FluentInjections.Internal.Utils;

public static class SemaphoreSlimExtensions
{
    public static DisposableWait DisposableWait(this SemaphoreSlim semaphoreSlim)
    {
        return new DisposableWait(semaphoreSlim);
    }
}

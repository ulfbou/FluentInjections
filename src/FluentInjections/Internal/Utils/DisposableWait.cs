// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Internal.Utils;

public class DisposableWait : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    public SemaphoreSlim Semaphore => _semaphore;

    public DisposableWait(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
        _semaphore.Wait();
    }

    public void Dispose()
    {
        _semaphore.Release();
    }
}

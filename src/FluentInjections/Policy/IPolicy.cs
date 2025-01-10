// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Policy;

public interface IPolicy
{
    Task ExecuteAsync(Func<Task> action, CancellationToken? cancellationToken = default);
}

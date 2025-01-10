// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Context;

using System.Threading;

namespace FluentInjections.Policy;

public interface IPolicy<TBinding> : IPolicy where TBinding : IBinding
{
    Task ExecuteAsync<TContext>(TContext context, CancellationToken? cancellationToken = default)
        where TContext : IExecutionContext;
}

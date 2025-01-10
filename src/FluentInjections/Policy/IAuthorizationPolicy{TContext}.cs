// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using FluentInjections.Context;

namespace FluentInjections.Policy;

public interface IAuthorizationPolicy<TContext> : IAuthorizationPolicy where TContext : IContext
{
    Task AuthorizeAsync(Func<TContext, Task> action);
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Context;

namespace FluentInjections.Policy;

public interface IValidationPolicy<TContext> : IValidationPolicy where TContext : IContext
{
    Task ValidateAsync(Func<TContext, Task> action);
}

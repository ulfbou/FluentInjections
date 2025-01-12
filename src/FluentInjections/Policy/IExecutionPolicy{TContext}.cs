// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using FluentInjections.Context;

using System.ComponentModel.DataAnnotations;

namespace FluentInjections.Policies;

/// <summary>
/// Represents a policy that can be executed with a type safe context. 
/// </summary>
/// <typeparam name="TContext">The type of the context that is used to execute the policy.</typeparam>
public interface IExecutionPolicy<TContext> : IExecutionPolicy where TContext : IExecutionContext
{
    Task ExecuteAsync(Func<TContext, Task> action);
}

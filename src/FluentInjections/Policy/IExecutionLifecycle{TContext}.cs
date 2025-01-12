// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Context;

namespace FluentInjections.Policies;

/// <summary>
/// Represents a lifecycle that is executed before and after an operation, as well as on success and failure. 
/// </summary>
/// <typeparam name="TContext">The type of the context that is used to execute the lifecycle.</typeparam>
public interface IExecutionLifecycle<TContext> : ILifecycle<TContext> where TContext : class, IContext { }

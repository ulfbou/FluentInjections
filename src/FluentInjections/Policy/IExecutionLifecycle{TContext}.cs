// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Context;

namespace FluentInjections.Policy;

public interface IExecutionLifecycle<TContext> : ILifecycle<TContext> where TContext : class, IContext { }

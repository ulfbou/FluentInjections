// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Bindings;

namespace FluentInjections.Policy;

/// <summary>
/// Represents a policy that can be executed with a type safe context. 
/// </summary>
public interface IExecutionPolicy : IPolicy<IExecutionBinding>
{
    Task ExecuteAsync(Func<Task> action);
}

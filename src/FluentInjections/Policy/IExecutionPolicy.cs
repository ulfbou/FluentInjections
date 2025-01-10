// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Policy;

/// <summary>
/// Represents a policy for executing a function.
/// </summary>
public interface IExecutionPolicy
{
    Task ExecuteAsync(Func<Task> value);
}


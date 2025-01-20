// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using FluentInjections.Bindings;

namespace FluentInjections.Policy;

/// <summary>
/// Represents a policy that logs the execution of operations for monitoring, debugging, or auditing purposes. 
/// </summary>
public interface ILoggingPolicy : IPolicy<ILoggingBinding> { }

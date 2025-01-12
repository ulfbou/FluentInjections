// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Bindings;

namespace FluentInjections.Policies;

/// <summary>
/// Represents a policy that validates the execution of an operation.
/// </summary>
public interface IValidationPolicy : IPolicy<IValidationBinding> { }

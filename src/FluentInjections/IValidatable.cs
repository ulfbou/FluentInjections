// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

/// <summary>
/// Represents an interface for objects that can be validated.
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// Validates the current state of the object.
    /// </summary>
    /// <exception cref="ValidationException">Thrown when the object is in an invalid state.</exception>
    void Validate();
}

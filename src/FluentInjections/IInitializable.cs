// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

/// <summary>
/// Represents an interface for objects that require initialization.
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// Initializes the current instance.
    /// This method should be called to perform any setup or initialization tasks required by the instance.
    /// </summary>
    void Initialize();
}

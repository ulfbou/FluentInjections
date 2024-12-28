// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Tests.Utilities;

/// <summary>
/// Represents a class that provides methods to guard against invalid arguments.
/// </summary>
internal class ArgumentGuard
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified argument is null.
    /// </summary>
    internal static void NotNull(object? argument, string errorMessage)
    {
        if (argument is null) throw new ArgumentNullException(errorMessage);
    }
}
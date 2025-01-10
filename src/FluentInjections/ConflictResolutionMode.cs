// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

/// <summary>
/// Represents a conflict resolution mode that determines how conflicts are resolved when binding components.
/// </summary>
public enum ConflictResolutionMode
{
    Replace,
    WarnAndReplace,
    Prevent,
    Merge,
    Ignore
}

// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections;

/// <summary>
/// Represents a module type that defines a module and its associated interface.
/// </summary>
internal sealed record ModuleType(Type Module, Type Interface);

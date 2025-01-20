// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Bindings;

namespace FluentInjections.Policy;

/// <summary>
/// Represents a caching policy that can be executed with a type safe context.
/// </summary>
/// <typeparam name="TContext">The type of context to apply the caching policy to.</typeparam>
public interface ICachingPolicy : IPolicy<ICachingBinding> { }

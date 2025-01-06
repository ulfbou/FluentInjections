// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Validation;

using System.Reflection;

namespace FluentInjections.Extensions.Attributes;

public static class MiscellaneousAttributeExtensions
{
    /// <summary>
    /// Gets all types from the given assembly that are decorated with the specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to check for.</typeparam>
    /// <param name="assembly">The assembly to search.</param>
    /// <returns>An IEnumerable of types that are decorated with the specified attribute.</returns>
    public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(this Assembly assembly) where TAttribute : Attribute
    {
        Guard.NotNull(assembly, nameof(assembly));
        return assembly.GetTypes().Where(t => t.HasAttribute<TAttribute>());
    }

    /// <summary>
    /// Gets all members from the given type that are decorated with the specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to check for.</typeparam>
    /// <param name="type">The type to search.</param>
    /// <returns>An IEnumerable of members that are decorated with the specified attribute.</returns>
    public static IEnumerable<MemberInfo> GetMembersWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        Guard.NotNull(type, nameof(type));
        return type.GetMembers().Where(m => m.HasAttribute<TAttribute>());
    }
}

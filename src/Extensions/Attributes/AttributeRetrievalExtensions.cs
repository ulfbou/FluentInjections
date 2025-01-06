// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Validation;

using System.Reflection;

namespace FluentInjections.Extensions.Attributes;

public static class AttributeRetrievalExtensions
{
    /// <summary>
    /// Gets the first attribute of the specified type that is applied to the type.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <param name="type">The type to get the attribute from.</param>
    /// <returns>The attribute if found, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="type"/> is null.</exception>
    public static TAttribute? GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        Guard.NotNull(type, nameof(type));
        return Attribute.GetCustomAttribute(type, typeof(TAttribute)) as TAttribute;
    }

    /// <summary>
    /// Gets the first attribute of the specified type that is applied to the member.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <param name="memberInfo">The member to get the attribute from.</param>
    /// <returns>The attribute if found, otherwise null.</returns>
    public static TAttribute? GetAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
    {
        return Attribute.GetCustomAttribute(memberInfo, typeof(TAttribute)) as TAttribute;
    }

    /// <summary>
    /// Gets all attributes of the specified type that are applied to the type.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <param name="type">The type to get the attributes from.</param>
    /// <returns>The attributes if found, otherwise an empty collection.</returns>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type type) where TAttribute : Attribute
    {
        return Attribute.GetCustomAttributes(type, typeof(TAttribute)).Cast<TAttribute>();
    }

    /// <summary>
    /// Gets all attributes of the specified type that are applied to the member.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <param name="memberInfo">The member to get the attributes from.</param>
    /// <returns>The attributes if found, otherwise an empty collection.</returns>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
    {
        return Attribute.GetCustomAttributes(memberInfo, typeof(TAttribute)).Cast<TAttribute>();
    }

    /// <summary>
    /// Gets all attributes of the specified type that are applied to the type, filtered by the given predicate.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <param name="type">The type to get the attributes from.</param>
    /// <param name="predicate">The predicate to filter the attributes.</param>
    /// <returns>An IEnumerable of attributes that match the predicate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="type"/> or <paramref name="predicate"/> is null.</exception>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type type, Func<TAttribute, bool> predicate) where TAttribute : Attribute
    {
        Guard.NotNull(type, nameof(type));
        Guard.NotNull(predicate, nameof(predicate));
        return Attribute.GetCustomAttributes(type, typeof(TAttribute)).Cast<TAttribute>().Where(predicate);
    }

    /// <summary>
    /// Gets all attributes of the specified type that are applied to the member, filtered by the given predicate.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <param name="memberInfo">The member to get the attributes from.</param>
    /// <param name="predicate">The predicate to filter the attributes.</param>
    /// <returns>An IEnumerable of attributes that match the predicate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="memberInfo"/> or <paramref name="predicate"/> is null.</exception>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo memberInfo, Func<TAttribute, bool> predicate) where TAttribute : Attribute
    {
        Guard.NotNull(memberInfo, nameof(memberInfo));
        Guard.NotNull(predicate, nameof(predicate));
        return Attribute.GetCustomAttributes(memberInfo, typeof(TAttribute)).Cast<TAttribute>().Where(predicate);
    }

    /// <summary>
    /// Gets all members from all types in the assembly that are decorated with the specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to check for.</typeparam>
    /// <param name="assembly">The assembly to search.</param>
    /// <returns>An IEnumerable of members that are decorated with the specified attribute.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="assembly"/> is null.</exception>
    public static IEnumerable<MemberInfo> GetMembersWithAttribute<TAttribute>(this Assembly assembly) where TAttribute : Attribute
    {
        Guard.NotNull(assembly, nameof(assembly));
        return assembly.GetTypes()
                       .SelectMany(t => t.GetMembers().Where(m => m.HasAttribute<TAttribute>()));
    }

    /// <summary>
    /// Gets the first attribute of the specified type that is applied to the member.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <param name="memberInfo">The member to get the attribute from.</param>
    /// <returns>The attribute if found, otherwise <see langword="null"/>.</returns>
    public static TAttribute? GetAssemblyAttribute<TAttribute>(this Assembly assembly) where TAttribute : Attribute
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        return assembly.GetCustomAttribute<TAttribute>();
    }

}

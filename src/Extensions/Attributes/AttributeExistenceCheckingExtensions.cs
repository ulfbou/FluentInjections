// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Validation;

using System.Reflection;

namespace FluentInjections.Extensions.Attributes;

/// <summary>
/// Extension methods for working with attributes.
/// </summary>
public static class AttributeExistenceCheckingExtensions
{
    /// <summary>
    /// Checks if the specified type is decorated with the given attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to check for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is decorated with the specified attribute, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="type"/> is null.</exception>
    public static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        Guard.NotNull(type, nameof(type));
        return Attribute.IsDefined(type, typeof(TAttribute));
    }

    /// <summary>
    /// Checks if the specified type is decorated with all of the given attributes.
    /// </summary>
    /// <typeparam name="TAttribute">The base type of the attributes to check for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <param name="attributeTypes">The types of the attributes to check for.</param>
    /// <returns>True if the type is decorated with all of the specified attributes, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="type"/> or <paramref name="attributeTypes"/> is null.</exception>
    public static bool HasAttributes<TAttribute>(this Type type, params Type[] attributeTypes) where TAttribute : Attribute
    {
        Guard.NotNull(type, nameof(type));
        Guard.NotNull(attributeTypes, nameof(attributeTypes));

        foreach (var attrType in attributeTypes)
        {
            if (!Attribute.IsDefined(type, attrType))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the specified member is decorated with the given attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to check for.</typeparam>
    /// <param name="memberInfo">The member to check.</param>
    /// <returns>True if the member is decorated with the specified attribute, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="memberInfo"/> is null.</exception>
    public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
    {
        Guard.NotNull(memberInfo, nameof(memberInfo));
        return Attribute.IsDefined(memberInfo, typeof(TAttribute));
    }

    /// <summary>
    /// Checks if the specified member is decorated with all of the given attributes.
    /// </summary>
    /// <typeparam name="TAttribute">The base type of the attributes to check for.</typeparam>
    /// <param name="memberInfo">The member to check.</param>
    /// <param name="attributeTypes">The types of the attributes to check for.</param>
    /// <returns>True if the member is decorated with all of the specified attributes, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="memberInfo"/> or <paramref name="attributeTypes"/> is null.</exception>
    public static bool HasAttributes<TAttribute>(this MemberInfo memberInfo, params Type[] attributeTypes) where TAttribute : Attribute
    {
        Guard.NotNull(memberInfo, nameof(memberInfo));
        Guard.NotNull(attributeTypes, nameof(attributeTypes));

        foreach (var attrType in attributeTypes)
        {
            if (!Attribute.IsDefined(memberInfo, attrType))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets all types from the given assemblies that are decorated with the specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to check for.</typeparam>
    /// <param name="assemblies">The assemblies to search.</param>
    /// <returns>An IEnumerable of types that are decorated with the specified attribute.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="assemblies"/> is null.</exception>
    public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(this IEnumerable<Assembly> assemblies) where TAttribute : Attribute
    {
        Guard.NotNull(assemblies, nameof(assemblies));
        return assemblies.SelectMany(a => a.GetTypes()).Where(t => t.HasAttribute<TAttribute>());
    }

    /// <summary>
    /// Checks if the specified type is decorated with any of the given attributes.
    /// </summary>
    /// <typeparam name="TAttribute">The base type of the attributes to check for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <param name="attributeTypes">The types of the attributes to check for.</param>
    /// <returns>True if the type is decorated with any of the specified attributes, otherwise false.</returns>
    public static bool HasAnyAttribute<TAttribute>(this MemberInfo memberInfo, params Type[] attributeTypes) where TAttribute : Attribute
    {
        Guard.NotNull(memberInfo, nameof(memberInfo));
        Guard.NotNull(attributeTypes, nameof(attributeTypes));
        return attributeTypes.Any(attributeType => Attribute.IsDefined(memberInfo, attributeType));
    }

    /// <summary>
    /// Filters the specified types by the given attribute and predicate.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to filter by.</typeparam>
    /// <param name="types">The types to filter.</param>
    /// <param name="predicate">The predicate to filter the types.</param>
    /// <returns>An IEnumerable of types that are decorated with the specified attribute and match the predicate.</returns>
    public static IEnumerable<Type> FilterByAttribute<TAttribute>(this IEnumerable<Type> types, Func<TAttribute, bool> predicate) where TAttribute : Attribute
    {
        Guard.NotNull(types, nameof(types));
        Guard.NotNull(predicate, nameof(predicate));
        return types.Where(type => type.GetCustomAttributes<TAttribute>().Any(predicate));
    }
}

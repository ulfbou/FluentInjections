// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Validation;

using System.Reflection;

namespace FluentInjections.Extensions.Attributes;

public static class AttributePropertyRetrievalExtensions
{
    /// <summary>
    /// Gets the value of the specified property from the attribute instance.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="attribute">The attribute instance.</param>
    /// <param name="propertyName">The name of the property to get the value from.</param>
    /// <returns>The value of the property if found, otherwise the default value of the type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="attribute"/> or <paramref name="propertyName"/> is null.</exception>
    public static T? GetPropertyValue<T>(this Attribute attribute, string propertyName)
    {
        Guard.NotNull(attribute, nameof(attribute));
        Guard.NotNullOrEmpty(propertyName, nameof(propertyName));

        var propertyInfo = attribute.GetType().GetProperty(propertyName);
        return propertyInfo is not null && propertyInfo.GetValue(attribute) is T value ? value : default;
    }

    /// <summary>
    /// Gets the value of the specified property from the attribute instance.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <typeparam name="TProperty">The type of the property to get.</typeparam>
    /// <param name="type">The type to get the attribute from.</param>
    /// <param name="selector">The selector to get the property value.</param>
    /// <returns>The value of the property if found, otherwise the default value of the type.</returns>
    public static TProperty? GetAttributeValue<TAttribute, TProperty>(this Type type, Func<TAttribute, TProperty> selector) where TAttribute : Attribute
    {
        Guard.NotNull(type, nameof(type));
        Guard.NotNull(selector, nameof(selector));

        var attribute = type.GetCustomAttribute<TAttribute>();
        return attribute != null ? selector(attribute) : default;
    }

    /// <summary>
    /// Gets the value of the specified property from the attribute instance.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <typeparam name="TProperty">The type of the property to get.</typeparam>
    /// <param name="memberInfo">The member to get the attribute from.</param>
    /// <param name="selector">The selector to get the property value.</param>
    /// <returns>The value of the property if found, otherwise the default value of the type.</returns>
    public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this MemberInfo memberInfo, bool inherit) where TAttribute : Attribute
    {
        Guard.NotNull(memberInfo, nameof(memberInfo));
        return Attribute.GetCustomAttributes(memberInfo, typeof(TAttribute), inherit).Cast<TAttribute>();
    }
}


using System.Reflection;

namespace FluentInjections.Extensions.Attributes;

/// <summary>
/// Extension methods for working with attributes.
/// </summary>
public static class AttributeExtensions
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
        if (type == null) throw new ArgumentNullException(nameof(type));
        return Attribute.IsDefined(type, typeof(TAttribute));
    }

    /// <summary>
    /// Gets the first attribute of the specified type that is applied to the type.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <param name="type">The type to get the attribute from.</param>
    /// <returns>The attribute if found, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="type"/> is null.</exception>
    public static TAttribute GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        return Attribute.GetCustomAttribute(type, typeof(TAttribute)) as TAttribute;
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
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (attributeTypes == null) throw new ArgumentNullException(nameof(attributeTypes));
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
    /// Checks if the specified member is decorated with all of the given attributes.
    /// </summary>
    /// <typeparam name="TAttribute">The base type of the attributes to check for.</typeparam>
    /// <param name="memberInfo">The member to check.</param>
    /// <param name="attributeTypes">The types of the attributes to check for.</param>
    /// <returns>True if the member is decorated with all of the specified attributes, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="memberInfo"/> or <paramref name="attributeTypes"/> is null.</exception>
    public static bool HasAttributes<TAttribute>(this MemberInfo memberInfo, params Type[] attributeTypes) where TAttribute : Attribute
    {
        if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
        if (attributeTypes == null) throw new ArgumentNullException(nameof(attributeTypes));
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
    /// Gets all attributes of the specified type that are applied to the type, filtered by the given predicate.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute to get.</typeparam>
    /// <param name="type">The type to get the attributes from.</param>
    /// <param name="predicate">The predicate to filter the attributes.</param>
    /// <returns>An IEnumerable of attributes that match the predicate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="type"/> or <paramref name="predicate"/> is null.</exception>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type type, Func<TAttribute, bool> predicate) where TAttribute : Attribute
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
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
        if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
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
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        return assembly.GetTypes()
                   .SelectMany(t => t.GetMembers().Where(m => m.HasAttribute<TAttribute>()));
    }

    /// <summary>
    /// Gets the value of the specified property from the attribute instance.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="attribute">The attribute instance.</param>
    /// <param name="propertyName">The name of the property to get the value from.</param>
    /// <returns>The value of the property if found, otherwise the default value of the type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="attribute"/> or <paramref name="propertyName"/> is null.</exception>
    public static T GetPropertyValue<T>(this Attribute attribute, string propertyName)
    {
        if (attribute == null) throw new ArgumentNullException(nameof(attribute));
        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
        var propertyInfo = attribute.GetType().GetProperty(propertyName);
        return propertyInfo != null && propertyInfo.GetValue(attribute) is T value ? value : default;
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
        if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
        return assemblies.SelectMany(a => a.GetTypesWithAttribute<TAttribute>());
    }
}

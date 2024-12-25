using FluentInjections.Validation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentInjections.Extensions;

public static class TypeExtensions
{
    private static Type[]? NumericTypes;

    // Check if a type is compatible with another type
    private static readonly Dictionary<Type, Dictionary<Type, bool>> _compatibilityCache = new();

    /// <summary>
    /// Checks if a source type is compatible with a target type. 
    /// </summary>
    /// <param name="sourceType">The source type to check compatibility for.</param>
    /// <param name="targetType">The target type to check compatibility with.</param>
    /// <returns><see langword="true"/> if the source type is compatible with the target type; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This method checks if the source type is compatible with the target type.
    /// </para>
    /// <para>
    /// The compatibility check is performed by checking the following conditions:
    /// <list type="bullet">
    /// <item><description>Equality comparison using <see cref="Type.Equals(Type)"/>.</description></item>
    /// <item><description>Direct assignability check using <see cref="Type.IsAssignableFrom(Type)"/>.</description></item>
    /// <item><description>Generic type compatibility check using <see cref="Type.IsGenericType"/> and <see cref="Type.GetGenericTypeDefinition()"/>.</description></item>
    /// <item><description>Interface implementation check using the 'is' operator.</description></item>
    /// <item><description>Base type compatibility check.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The compatibility check is cached to improve performance.
    /// </para>
    /// </remarks>
    public static bool IsCompatibleWith(this Type sourceType, Type targetType)
    {
        if (sourceType is null || targetType is null) return false;

        // Check cache first
        if (_compatibilityCache.TryGetValue(sourceType, out var targetDict) && targetDict.TryGetValue(targetType, out var result))
        {
            return result;
        }

        if (sourceType.Equals(targetType))
        {
            CacheCompatibility(sourceType, targetType, true);
            return true;
        }

        // Direct assignability check
        if (targetType.IsAssignableFrom(sourceType))
        {
            CacheCompatibility(sourceType, targetType, true);
            return true;
        }

        // Generic type compatibility check
        if (sourceType.IsGenericType && targetType.IsGenericType)
        {
            Type sourceGenericType = sourceType.GetGenericTypeDefinition();
            Type targetGenericType = targetType.GetGenericTypeDefinition();

            if (sourceGenericType == targetGenericType)
            {
                Type[] sourceGenericArguments = sourceType.GetGenericArguments();
                Type[] targetGenericArguments = targetType.GetGenericArguments();
                bool allCompatible = sourceGenericArguments.Length == targetGenericArguments.Length &&
                                    sourceGenericArguments.Zip(targetGenericArguments, IsCompatibleWith).All(x => x);

                CacheCompatibility(sourceType, targetType, allCompatible);
                return allCompatible;
            }
        }

        // Interface implementation check using the 'is' operator
        if (targetType.IsInterface && sourceType.GetInterfaces().Any(iface => iface.IsGenericType ? iface.GetGenericTypeDefinition() == targetType : iface == targetType))
        {
            CacheCompatibility(sourceType, targetType, true);
            return true;
        }

        // Base type compatibility check
        Type? currentType = sourceType.BaseType;

        while (currentType != null)
        {
            if (IsCompatibleWith(currentType, targetType))
            {
                CacheCompatibility(sourceType, targetType, true);
                return true;
            }

            currentType = currentType.BaseType;
        }

        // Cache the result as false
        CacheCompatibility(sourceType, targetType, false);
        return false;
    }

    private static void CacheCompatibility(Type sourceType, Type targetType, bool result)
    {
        if (!_compatibilityCache.ContainsKey(sourceType))
        {
            _compatibilityCache[sourceType] = new Dictionary<Type, bool>();
        }

        _compatibilityCache[sourceType][targetType] = result;
    }

    /// <summary>
    /// Safely casts an object to the specified type. Returns default(T) if the cast fails.
    /// </summary>
    /// <typeparam name="TTarget">The target type to cast to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <param name="result">The casted object, or default(T) if the cast fails.</param>
    /// <returns><see langword="true"/> if the cast was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryConvertTo<TTarget>(this object source, out TTarget result) where TTarget : class
    {
        result = default!;

        if (source is TTarget directCastResult)
        {
            result = directCastResult;
            return true;
        }

        var targetType = typeof(TTarget);

        if (source is null)
        {
            if (targetType.IsNullable())
            {
                result = default!;
                return true;
            }
            else
            {
                return false;
            }
        }

        // Fallback to compatibility check reflection-based conversion if direct cast fails
        var sourceType = source.GetType();

        if (sourceType.IsCompatibleWith(targetType))
        {
            try
            {
                result = (TTarget)Convert.ChangeType(source, targetType)!;
                return true;
            }
            catch
            {
                // Handle more advanced conversions here
                return TryConvertToAdvanced(source, targetType, out result);
            }
        }

        return false;
    }

    private static bool TryConvertToAdvanced<TTarget>(object source, Type targetType, out TTarget result) where TTarget : class
    {
        // Handle more advanced conversions here
        result = default!;
        CacheCompatibility(source.GetType(), targetType, false);
        return false;
    }

    /// <summary>
    /// Checks if a type is nullable.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is nullable; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullable(this Type type)
        => Nullable.GetUnderlyingType(type) is not null;

    /// <summary>
    /// Checks if a type is derived from a base type.
    /// </summary>
    /// <typeparam name="TBase">The base type to check against.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is derived from the base type; otherwise, <see langword="false"/>.</returns>
    public static bool IsDerivedFrom<TBase>(this Type type)
        => IsDerivedFrom(type, typeof(TBase));

    /// <summary>
    /// Checks if a type is derived from a base type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="baseType">The base type to check against.</param>
    /// <returns><see langword="true"/> if the type is derived from the base type; otherwise, <see langword="false"/>.</returns>
    public static bool IsDerivedFrom(this Type type, Type baseType)
        => baseType.IsAssignableFrom(type) && type != baseType;

    /// <summary>
    /// Gets all interfaces implemented by a type.
    /// </summary>
    /// <param name="type">The type to get the interfaces for.</param>
    /// <returns>An enumerable collection of interfaces implemented by the type.</returns>
    public static IEnumerable<Type> GetAllInterfaces(this Type type)
        => type.GetInterfaces();

    /// <summary>
    /// Checks if a type implements an interface.
    /// </summary>
    /// <typeparam name="TInterface">The interface to check for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type implements the interface; otherwise, <see langword="false"/>.</returns>
    public static bool ImplementsInterface<TInterface>(this Type type)
        => typeof(TInterface).IsAssignableFrom(type);

    /// <summary>
    /// Checks if a type has an attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to check for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type has the attribute; otherwise, <see langword="false"/>.</returns>
    public static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        => type.GetCustomAttributes(typeof(TAttribute), inherit: true).Any();

    /// <summary>
    /// Gets the full name of a type.
    /// </summary>
    /// <param name="type">The type to get the full name for.</param>
    /// <returns>The full name of the type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the type is <see langword="null"/>.</exception>
    public static string GetFullName(this Type type)
        => type?.FullName ?? throw new ArgumentNullException(nameof(type));

    /// <summary>
    /// Gets the public instance methods of a type.
    /// </summary>
    /// <param name="type">The type to get the methods for.</param>
    /// <returns>An enumerable collection of public instance methods of the type.</returns>
    public static IEnumerable<MethodInfo> GetPublicMethods(this Type type)
        => type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

    /// <summary>
    /// Gets the public instance properties of a type.
    /// </summary>
    /// <param name="type">The type to get the properties for.</param>
    /// <returns>An enumerable collection of public instance properties of the type.</returns>
    public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

    /// <summary>
    /// Checks if a type has a default constructor.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type has a default constructor; otherwise, <see langword="false"/>.</returns>
    public static bool HasDefaultConstructor(this Type type)
        => type.GetConstructor(Type.EmptyTypes) != null && type.GetConstructor(Type.EmptyTypes).IsPublic;

    /// <summary>
    /// Checks if a type is a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a numeric type; otherwise, <see langword="false"/>.</returns>
    public static bool IsNumericType(this Type type)
    {
        NumericTypes ??= new[]
        {
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)
        };
        return NumericTypes.Contains(type);
    }

    /// <summary>
    /// Checks if a type is a generic type definition.
    /// </summary>
    /// <typeparam name="TType">The generic type to check for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a generic type; otherwise, <see langword="false"/>.</returns>
    public static bool IsGeneric<TType>(this Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(TType);

    /// <summary>
    /// Gets the base types of a type.
    /// </summary>
    /// <param name="type">The type to get the base types for.</param>
    /// <returns>An enumerable collection of base types of the type.</returns>
    public static IEnumerable<Type> GetBaseTypes(this Type type)
    {
        var currentType = type;

        while (currentType.BaseType != null)
        {
            yield return currentType.BaseType;
            currentType = currentType.BaseType;
        }
    }

    /// <summary>
    /// Gets the generic arguments of a type.
    /// </summary>
    /// <param name="type">The type to get the generic arguments for.</param>
    /// <returns>An enumerable collection of generic arguments of the type.</returns>
    public static IEnumerable<Type> GetGenericArguments(this Type type) => type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;

    /// <summary>
    /// Gets the method with the specified name and parameter types.
    /// </summary>
    /// <param name="type">The type to get the method from.</param>
    /// <param name="methodName">The name of the method to get.</param>
    /// <param name="parameterTypes">The parameter types of the method to get.</param>
    /// <returns>The method with the specified name and parameter types, if found; otherwise, <see langword="null"/>.</returns>
    public static MethodInfo? GetMethod(this Type type, string methodName, params Type[] parameterTypes)
        => type.GetMethod(methodName, parameterTypes);

    /// <summary>
    /// Gets the property with the specified name.
    /// </summary>
    /// <param name="type">The type to get the property from.</param>
    /// <param name="propertyName">The name of the property to get.</param>
    /// <returns>The property with the specified name, if found; otherwise, <see langword="null"/>.</returns>
    public static PropertyInfo? GetProperty(this Type type, string propertyName)
        => type.GetProperty(propertyName);

    /// <summary>
    /// Gets the constructors of a type that has the specified parameter types.
    /// </summary>
    /// <param name="type">The type to get the constructors for.</param>
    /// <param name="parameters">The parameter types of the constructors to get.</param>
    /// <returns>An enumerable collection of constructors of the type that have the specified parameter types.</returns>
    public static IEnumerable<ConstructorInfo> GetConstructors(this Type type, params Type[] parameters)
        => type.GetConstructors()
               .Where(c => parameters.All(p => c.GetParameters().Select(pr => pr.ParameterType).Contains(p)));

    /// <summary>
    /// Checks if a type is assignable to another type.
    /// </summary>
    /// <typeparam name="T">The type to check assignability to.</typeparam>
    /// <param name="type">The type to check assignability for.</param>
    /// <returns><see langword="true"/> if the type is assignable to the other type; otherwise, <see langword="false"/>.</returns>
    public static bool IsAssignableTo<T>(this Type type)
        => typeof(T).IsAssignableFrom(type);

    /// <summary>
    /// Checks if a type is assignable from another type.
    /// </summary>
    /// <typeparam name="T">The type to check assignability from.</typeparam>
    /// <param name="type">The type to check assignability for.</param>
    /// <returns><see langword="true"/> if the type is assignable from the other type; otherwise, <see langword="false"/>.</returns>
    public static bool IsAssignableFrom<T>(this Type type)
        => type.IsAssignableFrom(typeof(T));

    /// <summary>
    /// Checks if a type is defined with an attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to check for.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is defined with the attribute; otherwise, <see langword="false"/>.</returns>
    public static bool IsDefined<TAttribute>(this Type type) where TAttribute : Attribute
        => Attribute.IsDefined(type, typeof(TAttribute));

    /// <summary>
    /// Tries to create an instance of a type from the specified arguments.
    /// </summary>
    /// <param name="type">The type to create an instance of.</param>
    /// <param name="args">The arguments to pass to the constructor.</param>
    /// <returns>The created instance of the type, if successful; otherwise, <see langword="null"/>.</returns>
    public static object? TryCreateInstance(this Type type, params object[] args)
    {
        try
        {
            return Activator.CreateInstance(type, args);
        }
        catch
        {
            return null;
        }
    }
}

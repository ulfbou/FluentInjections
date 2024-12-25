using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentInjections.Extensions; 

public static class TypeExtensions
{
    // Check if a type is compatible with another type
    public static bool IsCompatibleWith(this Type sourceType, Type targetType)
    {
        if (sourceType == null || targetType == null) return false;

        // Check basic type compatibility
        if (targetType.IsAssignableFrom(sourceType)) return true;

        // Check generic type compatibility
        if (sourceType.IsGenericType && targetType.IsGenericType)
        {
            var sourceGenericType = sourceType.GetGenericTypeDefinition();
            var targetGenericType = targetType.GetGenericTypeDefinition();
            if (sourceGenericType == targetGenericType)
            {
                var sourceGenericArguments = sourceType.GetGenericArguments();
                var targetGenericArguments = targetType.GetGenericArguments();
                return sourceGenericArguments.Length == targetGenericArguments.Length &&
                       sourceGenericArguments.Zip(targetGenericArguments, IsCompatibleWith).All(x => x);
            }
        }

        // Check interface implementation
        if (targetType.IsInterface)
        {
            var interfaces = sourceType.GetInterfaces();
            foreach (var iface in interfaces)
            {
                if (IsCompatibleWith(iface, targetType))
                {
                    return true;
                }
            }
        }

        // Check base class compatibility
        var currentType = sourceType.BaseType;
        while (currentType != null)
        {
            if (IsCompatibleWith(currentType, targetType))
            {
                return true;
            }
            currentType = currentType.BaseType;
        }

        // Check nullable type compatibility
        if (Nullable.GetUnderlyingType(sourceType) != null && Nullable.GetUnderlyingType(targetType) != null)
        {
            return IsCompatibleWith(Nullable.GetUnderlyingType(sourceType), Nullable.GetUnderlyingType(targetType));
        }

        // Check array and collection type compatibility
        if (sourceType.IsArray && targetType.IsArray)
        {
            return IsCompatibleWith(sourceType.GetElementType(), targetType.GetElementType());
        }

        // Check conversion operator compatibility
        if (sourceType.HasConversionOperatorTo(targetType) || targetType.HasConversionOperatorTo(sourceType))
        {
            return true;
        }

        return false;
    }

    // Attempt to convert an object to a specified type if they are compatible
    public static bool TryConvertTo<TTarget>(this object source, out TTarget result) where TTarget : class
    {
        var sourceType = source.GetType();
        var targetType = typeof(TTarget);
        if (sourceType.IsCompatibleWith(targetType))
        {
            try
            {
                result = (TTarget)Convert.ChangeType(source, targetType);
                return true;
            }
            catch
            {
                // Conversion failed
            }
        }
        result = default(TTarget);
        return false;
    }

    // Check if a type has an explicit or implicit conversion operator to another type
    private static bool HasConversionOperatorTo(this Type fromType, Type toType)
    {
        return fromType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Any(m => (m.Name == "op_Explicit" || m.Name == "op_Implicit") && m.ReturnType == toType);
    }

    public static bool IsNullable(this Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    public static bool IsDerivedFrom(this Type type, Type baseType)
    {
        return baseType.IsAssignableFrom(type) && type != baseType;
    }

    public static IEnumerable<Type> GetAllInterfaces(this Type type)
    {
        return type.GetInterfaces();
    }

    public static bool ImplementsInterface<TInterface>(this Type type)
    {
        return typeof(TInterface).IsAssignableFrom(type);
    }

    public static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        return type.GetCustomAttributes(typeof(TAttribute), inherit: true).Any();
    }

    public static string GetFullName(this Type type)
    {
        return type.FullName;
    }

    public static IEnumerable<MethodInfo> GetPublicMethods(this Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
    }

    public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    public static bool HasDefaultConstructor(this Type type)
    {
        return type.GetConstructor(Type.EmptyTypes) != null;
    }

    public static IEnumerable<FieldInfo> GetPublicFields(this Type type)
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
    }

    public static bool IsNumericType(this Type type)
    {
        return new[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) }.Contains(type);
    }

    public static bool IsGeneric<TType>(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(TType);
    }

    public static IEnumerable<Type> GetBaseTypes(this Type type)
    {
        var currentType = type;
        while (currentType.BaseType != null)
        {
            yield return currentType.BaseType;
            currentType = currentType.BaseType;
        }
    }
    
    public static IEnumerable<Type> GetGenericArguments(this Type type)
    {
        return type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;
    }

    public static MethodInfo GetMethod(this Type type, string methodName, params Type[] parameterTypes)
    {
        return type.GetMethod(methodName, parameterTypes);
    }

    public static PropertyInfo GetProperty(this Type type, string propertyName)
    {
        return type.GetProperty(propertyName);
    }
    
    public static IEnumerable<ConstructorInfo> GetConstructors(this Type type, params Type[] parameters)
    {
        return type.GetConstructors()
                   .Where(c => parameters.All(p => c.GetParameters().Select(pr => pr.ParameterType).Contains(p)));
    }
    
    public static bool IsAssignableTo<T>(this Type type)
    {
        return typeof(T).IsAssignableFrom(type);
    }

    public static bool IsDefined<TAttribute>(this Type type) where TAttribute : Attribute
    {
        return Attribute.IsDefined(type, typeof(TAttribute));
    }

    public static object? TryCreateInstance(this Type type, params object[] args)
    {
        try{
            return Activator.CreateInstance(type, args);
        catch{
            return null;} 
    }

    public static IEnumerable<Type> GetNestedTypes(this Type type)
    {
        return type.GetNestedTypes();
    }

    public static bool HasPublicParameterlessConstructor(this Type type)
    {
        return type.GetConstructor(Type.EmptyTypes) != null && type.GetConstructor(Type.EmptyTypes).IsPublic;
    }








} 

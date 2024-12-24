namespace FluentInjections.Extensions;


public static class TypeExtensions 
{
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

    public static IEnumerable<EventInfo> GetPublicEvents(this Type type)
    {
        return type.GetEvents(BindingFlags.Public | BindingFlags.Instance);
    }

    public static IEnumerable<ConstructorInfo> GetConstructorsWithMatchingParameters(this Type type, params Type[] parameters)
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

    public static object CreateInstance(this Type type, params object[] args)
    {
        return Activator.CreateInstance(type, args);
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

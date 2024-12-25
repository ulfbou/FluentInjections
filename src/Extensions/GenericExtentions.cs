namespace FluentInjections.Extensions;

public static class GenericExtensions
{
    /// <summary>
    /// Safely casts an object to the specified type. Returns default(T) if the cast fails.
    /// </summary>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <returns>The casted object, or default(T) if the cast fails.</returns>
    public static T As<T>(this object obj) where T : class
    {
        return obj as T ?? default(T)!;
    }

    /// <summary>
    /// Safely casts an object to the specified type. Returns null if the cast fails.
    /// </summary>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <returns>The casted object, or null if the cast fails.</returns>
    public static T AsNullable<T>(this object obj) where T : class
    {
        return (obj as T)!;
    }

    /// <summary>
    /// Executes an action if the object is not null.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to check.</param>
    /// <param name="action">The action to execute.</param>
    public static void IfNotNull<T>(this T obj, Action<T> action) where T : class
    {
        if (obj is not null)
        {
            action(obj);
        }
    }

    /// <summary>
    /// Executes a function and returns the result if the object is not null. 
    /// Returns default(TResult) if the object is null.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="obj">The object to check.</param>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function, or default(TResult) if the object is null.</returns>
    public static TResult IfNotNull<T, TResult>(this T obj, Func<T, TResult> func) where T : class
    {
        if (obj != null)
        {
            return func(obj);
        }

        return default(TResult)!;
    }

    /// <summary>
    /// Executes a function and returns the result if the object is not null. 
    /// Returns null if the object is null.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="obj">The object to check.</param>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function, or null if the object is null.</returns>
    public static TResult IfNotNullNullable<T, TResult>(this T obj, Func<T, TResult> func) where T : class where TResult : class
    {
        if (obj != null)
        {
            return func(obj);
        }

        return null!;
    }

    /// <summary>
    /// Checks if an object is of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to check for.</typeparam>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object is of the specified type, otherwise false.</returns>
    public static bool Is<T>(this object obj)
    {
        return obj is T;
    }
}

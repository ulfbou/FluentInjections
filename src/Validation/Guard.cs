// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Validation;

/// <summary>
/// A static class that provides methods to guard against null, empty, whitespace, negative, and zero values.
/// </summary>
/// <remarks>
/// This class should be used to validate method arguments.
/// </remarks>
public static class Guard
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">The value is <see langword="null"/>.</exception>
    public static void Null<T>(T value, string name)
    {
        if (value is not null)
        {
            throw new ArgumentNullException(name, "Value must be null.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">The value is <see langword="null"/>.</exception>
    public static void NotNull<T>(T value, string name)
    {
        if (value is null)
        {
            throw new ArgumentNullException(name, "Value cannot be null.");
        }
    }
    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the value is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentException">The value is <see langword="null"/> or empty.</exception>
    public static void NotNullOrEmpty(string value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the value is <see langword="null"/> or whitespace.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentException">The value is <see langword="null"/> or whitespace.</exception>
    public static void NotNullOrWhiteSpace(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", name);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is negative.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public static void NotNegative(IComparable value, string name)
    {
        if (value.CompareTo(0) < 0)
        {
            throw new ArgumentOutOfRangeException(name, "Value cannot be negative.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is negative or zero.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative or zero.</exception>
    public static void NotNegativeOrZero(IComparable value, string name)
    {
        if (value.CompareTo(0) <= 0)
        {
            throw new ArgumentOutOfRangeException(name, "Value cannot be negative or zero.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is zero.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value is zero.</exception>
    public static void NotZero(IComparable value, string name)
    {
        if (value.CompareTo(0) == 0)
        {
            throw new ArgumentOutOfRangeException(name, "Value cannot be zero.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the value is <see langword="null"/> or empty.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentException">The value is <see langword="null"/> or empty.</exception>
    public static void NotNullOrEmpty<T>(IEnumerable<T> value, string name)
    {
        if (value is null || !value.Any())
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the collection is <see langword="null"/> or empty.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="name">The name of the collection.</param>
    /// <exception cref="ArgumentException">The collection is <see langword="null"/> or empty.</exception>
    public static void NotNullOrEmpty<T>(ICollection<T> collection, string name)
    {
        if (collection is null || collection.Count == 0)
        {
            throw new ArgumentException("Collection cannot be null or empty.", name);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the collection is <see langword="null"/> or empty.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="name">The name of the collection.</param>
    /// <exception cref="ArgumentException">The collection is <see langword="null"/> or empty.</exception>
    public static void NotNullOrEmpty<T>(IReadOnlyCollection<T> collection, string name)
    {
        if (collection is null || collection.Count == 0)
        {
            throw new ArgumentException("Collection cannot be null or empty.", name);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the list is <see langword="null"/> or empty.
    /// </summary>
    /// <typeparam name="T">The type of the list.</typeparam>
    /// <param name="list">The list to check.</param>
    /// <param name="name">The name of the list.</param>
    /// <exception cref="ArgumentException">The list is <see langword="null"/> or empty.</exception>
    public static void NotNullOrEmpty<T>(IReadOnlyList<T> list, string name)
    {
        if (list is null || list.Count == 0)
        {
            throw new ArgumentException("List cannot be null or empty.", name);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the list is <see langword="null"/> or empty.
    /// </summary>
    /// <typeparam name="T">The type of the list.</typeparam>
    /// <param name="list">The list to check.</param>
    /// <param name="name">The name of the list.</param>
    /// <exception cref="ArgumentException">The list is <see langword="null"/> or empty.</exception>
    public static void NotNullOrEmpty<T>(IList<T> list, string name)
    {
        if (list is null || list.Count == 0)
        {
            throw new ArgumentException("List cannot be null or empty.", name);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the dictionary is not defined in the enum.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="dictionary">The dictionary to check.</param>
    /// <param name="name">The name of the dictionary.</param>
    /// <exception cref="ArgumentException">The value is not defined in the enum.</exception>
    public static void NotNullOrEmpty<TKey, TValue>(IDictionary<TKey, TValue> dictionary, string name)
    {
        if (dictionary is null || dictionary.Count == 0)
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is not defined in the enum.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value is not defined in the enum.</exception>
    public static void InRange<TEnum>(TEnum value, string name) where TEnum : Enum
    {
        if (!Enum.IsDefined(typeof(TEnum), value))
        {
            throw new ArgumentOutOfRangeException(name, "Value is not defined in the enum.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the value is not assignable to the expected type.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <exception cref="ArgumentException">The actual type is not assignable to the expected type.</exception>
    public static void IsType<T>(object instance) where T : class
    {
        Guard.NotNull(instance, nameof(instance));

        var expectedType = typeof(T);
        var actualType = instance.GetType();

        if (!expectedType.IsAssignableFrom(actualType))
        {
            throw new ArgumentException("The actual type {actualType.Name} is not assignable to the expected type {expectedType.Name}.");
        }
    }
}

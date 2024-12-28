// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace FluentInjections.Validation;

public static class Guard
{
    public static void NotNull<T>(T value, string name)
    {
        if (value is null)
        {
            throw new ArgumentNullException(name, "Value cannot be null.");
        }
    }

    public static void NotNullOrEmpty(string value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }

    public static void NotNullOrWhiteSpace(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", name);
        }
    }

    public static void NotNegative(IComparable value, string name)
    {
        if (value.CompareTo(0) < 0)
        {
            throw new ArgumentOutOfRangeException(name, "Value cannot be negative.");
        }
    }

    public static void NotNegativeOrZero(IComparable value, string name)
    {
        if (value.CompareTo(0) <= 0)
        {
            throw new ArgumentOutOfRangeException(name, "Value cannot be negative or zero.");
        }
    }

    public static void NotZero(IComparable value, string name)
    {
        if (value.CompareTo(0) == 0)
        {
            throw new ArgumentOutOfRangeException(name, "Value cannot be zero.");
        }
    }

    public static void NotNullOrEmpty<T>(IEnumerable<T> value, string name)
    {
        if (value is null || !value.Any())
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }

    public static void NotNullOrEmpty<T>(ICollection<T> value, string name)
    {
        if (value is null || value.Count == 0)
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }

    public static void NotNullOrEmpty<T>(IReadOnlyCollection<T> value, string name)
    {
        if (value is null || value.Count == 0)
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }

    public static void NotNullOrEmpty<T>(IReadOnlyList<T> value, string name)
    {
        if (value is null || value.Count == 0)
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }

    public static void NotNullOrEmpty<T>(IList<T> value, string name)
    {
        if (value is null || value.Count == 0)
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }

    public static void NotNullOrEmpty<TKey, TValue>(IDictionary<TKey, TValue> value, string name)
    {
        if (value is null || value.Count == 0)
        {
            throw new ArgumentException("Value cannot be null or empty.", name);
        }
    }
}

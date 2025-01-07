// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Validation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Extensions;

public static class DictionaryExtensions
{
    /// <summary>
    /// Creates an empty dictionary of the same type as the specified dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to create an empty dictionary from.</param>
    /// <returns>An empty dictionary of the same type as the specified dictionary.</returns>
    /// <exception cref="ArgumentNullException">The dictionary is <see langword="null"/>.</exception>
    public static IDictionary<TKey, TValue> Empty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        Guard.NotNull(dictionary, nameof(dictionary));

        return dictionary switch
        {
            Dictionary<TKey, TValue> => new Dictionary<TKey, TValue>(),
            SortedDictionary<TKey, TValue> => new SortedDictionary<TKey, TValue>(),
            _ => throw new NotSupportedException("Unsupported dictionary type.")
        };
    }

    /// <summary>
    /// Merges the specified dictionaries into a new dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionaries.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionaries.</typeparam>
    /// <param name="dictionary">The dictionary to merge into.</param>
    /// <param name="otherDictionaries">The other dictionaries to merge.</param>
    /// <returns>A new dictionary that contains the merged key-value pairs.</returns>
    /// <exception cref="ArgumentNullException">The dictionary or other dictionary is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">The dictionary type is not supported.</exception>
    public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, params IDictionary<TKey, TValue>[] otherDictionaries)
        where TKey : notnull
    {
        Guard.NotNull(dictionary, nameof(dictionary));
        Guard.NotNull(otherDictionaries, nameof(otherDictionaries));

        var merged = dictionary.Empty();

        foreach (var otherDictionary in otherDictionaries)
        {
            foreach (var (key, value) in otherDictionary)
            {
                merged[key] = value;
            }
        }

        foreach (var (key, value) in dictionary)
        {
            merged[key] = value;
        }

        return merged;
    }
}

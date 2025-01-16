// Copyright (c) FluentInjections Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentInjections.Validation;

namespace FluentInjections.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Filters the elements of an <see cref="IEnumerable{T}"/> that are not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to filter.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains elements from the input sequence that are not <see langword="null"/>.</returns>
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?>? source) where T : class
    {
        if (source is null)
        {
            return Enumerable.Empty<T>();
        }

        return source.Where(item => item is not null)!;
    }

    // TODO: Ideas for versatile extension methods:
    // - 
    // - IfNotNull: Filters the elements of an <see cref="IEnumerable{T}"/> that are not <see langword="null"/>.
    // - IfNotNullOrEmpty: Filters the elements of an <see cref="IEnumerable{T}"/> that are not <see langword="null"/> or empty.
    // - IfNotNullOrWhiteSpace: Filters the elements of an <see cref="IEnumerable{T}"/> that are not <see langword="null"/> or white space.
    // - 
}

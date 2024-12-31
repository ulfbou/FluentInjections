using FluentInjections.Validation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInjections.Extensions;

public static class DictionaryExtensions
{
    // Dictionary.Empty
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
}

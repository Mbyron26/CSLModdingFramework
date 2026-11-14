using System;
using System.Collections.Generic;
using CSLModdingFramework.Collections;

namespace CSLModdingFramework.Extensions;

public static class CollectionExtensions {
    public static IReadOnlyCollection<T> AsReadOnlyCollection<T>(this ICollection<T> source) => source == null ? throw new ArgumentNullException(nameof(source)) : new ReadOnlyCollectionWrapper<T>(source);


    public static IReadOnlyList<T> AsReadOnlyList<T>(this IList<T> source) => source == null ? throw new ArgumentNullException(nameof(source)) : new ReadOnlyListWrapper<T>(source);

    public static IReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dict) => dict == null ? throw new ArgumentNullException(nameof(dict)) : new ReadOnlyDictionaryWrapper<TKey, TValue>(dict);
}
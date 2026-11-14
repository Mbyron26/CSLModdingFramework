using System;

namespace CSLModdingFramework.Extensions;

public static class ArrayExtensions {
    public static T[] Empty<T>() => Cache<T>.Value;

    public static bool IsNullOrEmpty<T>(this T[] array) => array == null || array.Length == 0;

    public static string ToJoinedString<T>(this T[] array, string separator = ",") => array == null ? string.Empty : string.Join(separator, Array.ConvertAll(array, item => item?.ToString()));

    internal static class Cache<T> {
        internal static readonly T[] Value = new T[0];
    }
}

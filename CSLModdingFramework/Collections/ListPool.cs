using System.Collections.Generic;

namespace CSLModdingFramework.Collections;

public static class ListPool<T> {
    private static readonly Stack<List<T>> Pool = new();

    public static int MaxPoolSize { get; set; } = 64;
    public static int MaxCapacity { get; set; } = 4096;
    public static int Count => Pool.Count;

    public static List<T> Rent(int capacity = 4) {
        List<T> list;

        if (Pool.Count > 0) {
            list = Pool.Pop();

            if (list.Capacity < capacity)
                list.Capacity = capacity;
        }
        else {
            list = new List<T>(capacity);
        }

        return list;
    }

    public static void Return(List<T> list) {
        if (list == null)
            return;

        list.Clear();

        if (list.Capacity > MaxCapacity)
            return;

        if (Pool.Count >= MaxPoolSize)
            return;

        Pool.Push(list);
    }

    public static void ClearPool() => Pool.Clear();
}
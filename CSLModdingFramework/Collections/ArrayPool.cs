using System.Text;
using System;
using System.Collections.Generic;

namespace CSLModdingFramework.Collections;

public sealed class ArrayPool<T> {
    private const int DefaultMaxArrayLength = 1024 * 1024;
    private const int DefaultMaxPerBucket = 128;

    private readonly Stack<T[]>[] _buckets;
    private readonly int _maxArrayLength;
    private readonly int _maxPerBucket;

    public static readonly ArrayPool<T> Shared = new();

    public ArrayPool(int maxArrayLength = DefaultMaxArrayLength, int maxPerBucket = DefaultMaxPerBucket) {
        _maxArrayLength = maxArrayLength;
        _maxPerBucket = maxPerBucket;
        _buckets = new Stack<T[]>[GetBucketIndex(maxArrayLength) + 1];
    }

    public T[] Rent(int minimumLength) {
        if (minimumLength < 1)
            minimumLength = 1;

        var bucket = GetBucketIndex(minimumLength);

        var stack = _buckets[bucket];

        if (stack != null && stack.Count > 0)
            return stack.Pop();

        return new T[GetBucketSize(bucket)];
    }

    public void Return(T[] array, bool clearArray = false) {
        if (array == null) return;

        var length = array.Length;
        if (length > _maxArrayLength) return;

        var bucket = GetBucketIndex(length);
        if (GetBucketSize(bucket) != length) return;

        if (clearArray) {
            Array.Clear(array, 0, array.Length);
        }

        var stack = _buckets[bucket];
        if (stack == null) {
            stack = new Stack<T[]>(_maxPerBucket);
            _buckets[bucket] = stack;
        }

        if (stack.Count < _maxPerBucket) {
            stack.Push(array);
        }
    }

    public string DumpStatistics() {
        var sb = new StringBuilder();
        sb.AppendLine($"ArrayPool<{typeof(T).Name}>");
        sb.AppendLine($"MaxArrayLength: {_maxArrayLength}");
        sb.AppendLine($"MaxPerBucket: {_maxPerBucket}");
        sb.AppendLine("Buckets:");

        for (var i = 0; i < _buckets.Length; i++) {
            var stack = _buckets[i];
            var count = stack?.Count ?? 0;

            if (count == 0)
                continue;

            sb.AppendLine($"Size={GetBucketSize(i)}, Count={count}");
        }

        return sb.ToString();
    }

    private static int GetBucketSize(int bucket) => 1 << bucket;

    private static int GetBucketIndex(int length) {
        if (length <= 1) return 0;

        var index = 0;
        var size = 1;

        while (size < length) {
            size <<= 1;
            index++;
        }

        return index;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

namespace CSLModdingFramework.Collections;

public class ReadOnlyListWrapper<T> : IReadOnlyList<T> {
    private readonly IList<T> _list;

    public int Count => _list.Count;

    public ReadOnlyListWrapper(IList<T> list) => _list = list ?? throw new ArgumentNullException(nameof(list));

    public T this[int index] => _list[index];

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
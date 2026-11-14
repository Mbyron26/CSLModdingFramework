using System.Collections.Generic;

namespace CSLModdingFramework.Collections;

public interface IReadOnlyCollection<T> : IEnumerable<T> {
    int Count { get; }
}
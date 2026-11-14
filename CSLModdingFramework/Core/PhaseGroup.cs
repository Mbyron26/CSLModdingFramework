using System.Collections.Generic;

namespace CSLModdingFramework.Core;

internal sealed class PhaseGroup<T> {
    public readonly List<T> Before = new();
    public readonly List<T> At = new();
    public readonly List<T> After = new();
}
using System.Collections.Generic;

namespace CSLModdingFramework.Core;

internal sealed class UpdatePhaseBucket {
    public readonly List<SystemBase> Before = new();
    public readonly List<SystemBase> At = new();
    public readonly List<SystemBase> After = new();
}

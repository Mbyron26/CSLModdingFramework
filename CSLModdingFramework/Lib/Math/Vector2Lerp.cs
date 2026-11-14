using UnityEngine;

namespace CSLModdingFramework.Lib.Math;

public struct Vector2Lerp : ILerp<Vector2> {
    public Vector2 Lerp(in Vector2 a, in Vector2 b, float t) => new(
        a.x + (b.x - a.x) * t,
        a.y + (b.y - a.y) * t
    );
}
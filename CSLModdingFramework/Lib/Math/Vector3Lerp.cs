using UnityEngine;

namespace CSLModdingFramework.Lib.Math;

public struct Vector3Lerp : ILerp<Vector3> {
    public Vector3 Lerp(in Vector3 a, in Vector3 b, float t) => new(
        a.x + (b.x - a.x) * t,
        a.y + (b.y - a.y) * t,
        a.z + (b.z - a.z) * t
    );
}
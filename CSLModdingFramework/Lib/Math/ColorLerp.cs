using UnityEngine;

namespace CSLModdingFramework.Lib.Math;

public struct ColorLerp : ILerp<Color> {
    public Color Lerp(in Color a, in Color b, float t) => new(
        a.r + (b.r - a.r) * t,
        a.g + (b.g - a.g) * t,
        a.b + (b.b - a.b) * t,
        a.a + (b.a - a.a) * t
    );
}
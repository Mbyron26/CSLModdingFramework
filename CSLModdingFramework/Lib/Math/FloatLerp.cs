namespace CSLModdingFramework.Lib.Math;

public struct FloatLerp : ILerp<float> {
    public float Lerp(in float a, in float b, float t) => a + (b - a) * t;
}

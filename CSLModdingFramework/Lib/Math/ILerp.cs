namespace CSLModdingFramework.Lib.Math;

public interface ILerp<T> {
    T Lerp(in T a, in T b, float t);
}
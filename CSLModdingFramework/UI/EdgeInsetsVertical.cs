using UnityEngine;

namespace CSLModdingFramework.UI;

public struct EdgeInsetsVertical {
    public float Top;
    public float Bottom;

    public float Vertical => Top + Bottom;

    public EdgeInsetsVertical() { }

    public EdgeInsetsVertical(float uniform) => Top = Bottom = uniform;

    public EdgeInsetsVertical(float top, float bottom) {
        Top = top;
        Bottom = bottom;
    }

    public static bool operator ==(EdgeInsetsVertical a, EdgeInsetsVertical b) => a.Equals(b);

    public static bool operator !=(EdgeInsetsVertical a, EdgeInsetsVertical b) => !a.Equals(b);

    public override string ToString() => $"Left: {Top}, Right: {Bottom}";
    public override bool Equals(object obj) => obj is EdgeInsetsVertical other && Equals(other);

    public override int GetHashCode() {
        unchecked {
            return (Top.GetHashCode() * 397) ^ Bottom.GetHashCode();
        }
    }

    public bool Equals(EdgeInsetsVertical other) => EqualsApprox(other);

    public bool EqualsApprox(EdgeInsetsVertical other, float epsilon = 0.0001f) =>
        Mathf.Abs(Top - other.Top) <= epsilon &&
        Mathf.Abs(Bottom - other.Bottom) <= epsilon;
}
using System;
using UnityEngine;

namespace CSLModdingFramework.UI;

public struct EdgeInsetsHorizontal : IEquatable<EdgeInsetsHorizontal> {
    public static readonly EdgeInsetsHorizontal Zero = new(0f);

    public float Left;
    public float Right;

    public float Horizontal => Left + Right;

    public EdgeInsetsHorizontal() { }

    public EdgeInsetsHorizontal(float uniform) => Left = Right = uniform;

    public EdgeInsetsHorizontal(float left, float right) {
        Left = left;
        Right = right;
    }

    public static bool operator ==(EdgeInsetsHorizontal a, EdgeInsetsHorizontal b) => a.Equals(b);

    public static bool operator !=(EdgeInsetsHorizontal a, EdgeInsetsHorizontal b) => !a.Equals(b);

    public override string ToString() => $"Left: {Left}, Right: {Right}";
    public override bool Equals(object obj) => obj is EdgeInsetsHorizontal other && Equals(other);

    public override int GetHashCode() {
        unchecked {
            return (Left.GetHashCode() * 397) ^ Right.GetHashCode();
        }
    }

    public bool Equals(EdgeInsetsHorizontal other) => EqualsApprox(other);

    public bool EqualsApprox(EdgeInsetsHorizontal other, float epsilon = 0.0001f) =>
        Mathf.Abs(Left - other.Left) <= epsilon &&
        Mathf.Abs(Right - other.Right) <= epsilon;
}
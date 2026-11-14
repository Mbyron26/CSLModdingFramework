using System;
using UnityEngine;

namespace CSLModdingFramework.UI;

public struct EdgeInsets : IEquatable<EdgeInsets> {
    public static readonly EdgeInsets Zero = new(0f);

    public float Left;
    public float Right;
    public float Top;
    public float Bottom;

    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;
    public bool IsZero => Left == 0f && Right == 0f && Top == 0f && Bottom == 0f;

    public EdgeInsets(float left, float right, float top, float bottom) {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    public EdgeInsets(float uniform) => Left = Right = Top = Bottom = uniform;

    public EdgeInsets(float horizontal, float vertical) {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }

    public float this[int index] {
        get => index switch {
            0 => Left,
            1 => Right,
            2 => Top,
            3 => Bottom,
            _ => throw new IndexOutOfRangeException("EdgeInsets index out of range")
        };
        set {
            switch (index) {
                case 0: Left = value; break;
                case 1: Right = value; break;
                case 2: Top = value; break;
                case 3: Bottom = value; break;
                default: throw new IndexOutOfRangeException("EdgeInsets index out of range");
            }
        }
    }

    public static EdgeInsets operator +(EdgeInsets a, EdgeInsets b) => new(
        a.Left + b.Left,
        a.Right + b.Right,
        a.Top + b.Top,
        a.Bottom + b.Bottom
    );

    public static EdgeInsets operator -(EdgeInsets a, EdgeInsets b) => new(
        a.Left - b.Left,
        a.Right - b.Right,
        a.Top - b.Top,
        a.Bottom - b.Bottom
    );

    public static EdgeInsets operator *(EdgeInsets a, float scale) => new(
        a.Left * scale,
        a.Right * scale,
        a.Top * scale,
        a.Bottom * scale
    );

    public static EdgeInsets operator /(EdgeInsets a, float scale) {
        var inv = 1f / scale;
        return new EdgeInsets(
            a.Left * inv,
            a.Right * inv,
            a.Top * inv,
            a.Bottom * inv
        );
    }

    public static bool operator ==(EdgeInsets a, EdgeInsets b) => a.Equals(b);

    public static bool operator !=(EdgeInsets a, EdgeInsets b) => !a.Equals(b);

    public static EdgeInsets FromRectOffset(RectOffset offset) => new(
        offset.left,
        offset.right,
        offset.top,
        offset.bottom
    );

    public override string ToString() => $"L:{Left}, R:{Right}, T:{Top}, B:{Bottom}";
    public override bool Equals(object obj) => obj is EdgeInsets other && Equals(other);

    public override int GetHashCode() {
        unchecked {
            var hash = Left.GetHashCode();
            hash = (hash * 397) ^ Right.GetHashCode();
            hash = (hash * 397) ^ Top.GetHashCode();
            hash = (hash * 397) ^ Bottom.GetHashCode();
            return hash;
        }
    }

    public bool Equals(EdgeInsets other) => EqualsApprox(other);

    public bool EqualsApprox(EdgeInsets other, float epsilon = 0.0001f) =>
        Mathf.Abs(Left - other.Left) <= epsilon &&
        Mathf.Abs(Right - other.Right) <= epsilon &&
        Mathf.Abs(Top - other.Top) <= epsilon &&
        Mathf.Abs(Bottom - other.Bottom) <= epsilon;

    public void Set(float left, float right, float top, float bottom) {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    public void Scale(float scale) {
        Left *= scale;
        Right *= scale;
        Top *= scale;
        Bottom *= scale;
    }

    public EdgeInsets Scaled(float scale) => new(
        Left * scale,
        Right * scale,
        Top * scale,
        Bottom * scale
    );

    public EdgeInsets Add(EdgeInsets other) => new(
        Left + other.Left,
        Right + other.Right,
        Top + other.Top,
        Bottom + other.Bottom
    );

    public EdgeInsets Subtract(EdgeInsets other) => new(
        Left - other.Left,
        Right - other.Right,
        Top - other.Top,
        Bottom - other.Bottom
    );

    public RectOffset ToRectOffset() => new(
        Mathf.RoundToInt(Left),
        Mathf.RoundToInt(Right),
        Mathf.RoundToInt(Top),
        Mathf.RoundToInt(Bottom)
    );
}
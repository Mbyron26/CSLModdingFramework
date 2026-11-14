using System;
using UnityEngine;

namespace CSLModdingFramework.UI;

public readonly struct Size : IEquatable<Size> {
    public static readonly Size Zero = new(0, 0);
    public static readonly Size Infinity = new(float.PositiveInfinity, float.PositiveInfinity);

    public float Width { get; }
    public float Height { get; }
    public bool IsZero => Width == 0f && Height == 0f;
    public bool IsFullyInfinite => float.IsInfinity(Width) && float.IsInfinity(Height);
    public bool HasInfinite => float.IsInfinity(Width) || float.IsInfinity(Height);

    public Size(float width, float height) {
        Width = Mathf.Max(0, width);
        Height = Mathf.Max(0, height);
    }

    public override bool Equals(object obj) => obj is Size other && Equals(other);

    public override int GetHashCode() => Width.GetHashCode() ^ (Height.GetHashCode() << 2);

    public override string ToString() => $"Size({Width}, {Height})";
    public bool IsNearlyZero(float epsilon = 0.001f) => Approximately(this, Zero, epsilon);

    public Vector2 ToVector2() => new(Width, Height);

    public Size Clamp(Size min, Size max) => new(
        Mathf.Clamp(Width, min.Width, max.Width),
        Mathf.Clamp(Height, min.Height, max.Height)
    );

    public Size ExpandTo(Size other) => new(
        Mathf.Max(Width, other.Width),
        Mathf.Max(Height, other.Height)
    );

    public Size ShrinkTo(Size other) => new(
        Mathf.Min(Width, other.Width),
        Mathf.Min(Height, other.Height)
    );

    public bool Approximately(Size other, float epsilon = 0.001f) => Approximately(this, other, epsilon);

    public bool Equals(Size other) => Width == other.Width && Height == other.Height;

    public static bool Approximately(Size a, Size b, float epsilon = 0.001f) => Mathf.Abs(a.Width - b.Width) < epsilon && Mathf.Abs(a.Height - b.Height) < epsilon;

    public static implicit operator Vector2(Size s) => new(s.Width, s.Height);

    public static Size SubtractClamped(Size a, Size b) => new(Mathf.Max(0, a.Width - b.Width), Mathf.Max(0, a.Height - b.Height));

    public static Size FromVector2(Vector2 v) => new(v.x, v.y);

    public static Size operator +(Size a, Size b) => new(a.Width + b.Width, a.Height + b.Height);

    public static Size operator -(Size a, Size b) => new(a.Width - b.Width, a.Height - b.Height);

    public static Size operator *(Size a, float scale) => new(a.Width * scale, a.Height * scale);

    public static Size operator /(Size a, float scale) => new(a.Width / scale, a.Height / scale);

    public static bool operator >(Size a, Size b) => a.Width > b.Width && a.Height > b.Height;
    public static bool operator <(Size a, Size b) => a.Width < b.Width && a.Height < b.Height;
}
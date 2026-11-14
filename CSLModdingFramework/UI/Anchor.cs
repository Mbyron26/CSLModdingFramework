using System;
using UnityEngine;

namespace CSLModdingFramework.UI;

public readonly struct Anchor : IEquatable<Anchor> {
    public static readonly Anchor BottomCenter = new(0.5f, 1f);
    public static readonly Anchor BottomLeft = new(0f, 1f);
    public static readonly Anchor BottomRight = new(1f, 1f);
    public static readonly Anchor Center = new(0.5f, 0.5f);
    public static readonly Anchor MiddleLeft = new(0f, 0.5f);
    public static readonly Anchor MiddleRight = new(1f, 0.5f);
    public static readonly Anchor TopCenter = new(0.5f, 0f);
    public static readonly Anchor TopLeft = new(0f, 0f);
    public static readonly Anchor TopRight = new(1f, 0f);

    public readonly Vector2 Value;

    public Anchor(Vector2 value) => Value = new Vector2(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y));
    public Anchor(float x, float y) => Value = new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));

    public override bool Equals(object obj) => obj is Anchor other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => $"Anchor({Value.x:F2}, {Value.y:F2})";

    public Vector2 CalculateTopLeftAnchorOffset(Vector2 containerSize, Vector2 anchoredSize, Vector2 extraOffset) => CalculateTopLeftAnchorOffset(containerSize, anchoredSize) + extraOffset;

    public Vector2 CalculateTopLeftAnchorOffset(Vector2 containerSize, Vector2 contentSize) => new(
        (containerSize.x - contentSize.x) * Value.x,
        (containerSize.y - contentSize.y) * Value.y
    );

    public bool Equals(Anchor other) => Value.Equals(other.Value);

    public static implicit operator Anchor(Vector2 v) => new(v);

    public static implicit operator Vector2(Anchor a) => a.Value;
}
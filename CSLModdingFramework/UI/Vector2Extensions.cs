using UnityEngine;

namespace CSLModdingFramework.UI;

public static class Vector2Extensions {
    public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max) => new(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));

    public static Vector2 ClampMin(this Vector2 value, Vector2 min) => new(Mathf.Max(value.x, min.x), Mathf.Max(value.y, min.y));

    public static Vector2 ClampMax(this Vector2 value, Vector2 max) => new(Mathf.Min(value.x, max.x), Mathf.Min(value.y, max.y));
}
using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI;

public static class UIPivotPointExtensions {
    public static Vector3 GetUpperLeftOffset(this UIPivotPoint pivot, Vector2 size, Vector2 offset) => pivot switch {
        UIPivotPoint.TopLeft => new Vector3(0.0f + offset.x, 0.0f + offset.y),
        UIPivotPoint.TopCenter => new Vector3((float)(0.5 * -(double)size.x) + offset.x, 0.0f + offset.y),
        UIPivotPoint.TopRight => new Vector3(-size.x + offset.x, 0.0f + offset.y),
        UIPivotPoint.MiddleLeft => new Vector3(0.0f + offset.x, -0.5f * size.y + offset.y),
        UIPivotPoint.MiddleCenter => new Vector3((float)(0.5 * -(double)size.x) + offset.x, -0.5f * size.y + offset.y),
        UIPivotPoint.MiddleRight => new Vector3(-size.x + offset.x, -0.5f * size.y + offset.y),
        UIPivotPoint.BottomLeft => new Vector3(0.0f + offset.x, -size.y + offset.y),
        UIPivotPoint.BottomCenter => new Vector3((float)(0.5 * -(double)size.x) + offset.x, -size.y + offset.y),
        UIPivotPoint.BottomRight => new Vector3(-size.x + offset.x, -size.y + offset.y),
        UIPivotPoint.Arbitrary => new Vector3(offset.x, offset.y),
        _ => new Vector3(0.0f + offset.x, 0.0f + offset.y)
    };
}
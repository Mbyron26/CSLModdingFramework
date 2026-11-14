using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI;

public static class LayoutUtility {
    public static Vector3 CalculateAlignedOffset(UIHorizontalAlignment hAlign, UIVerticalAlignment vAlign, Vector2 availableSize, Vector2 contentSize, EdgeInsets padding) {
        var innerWidth = availableSize.x - padding.Horizontal;
        var innerHeight = availableSize.y - padding.Vertical;

        var offsetX = hAlign switch {
            UIHorizontalAlignment.Left => padding.Left,
            UIHorizontalAlignment.Right => padding.Left + (innerWidth - contentSize.x),
            _ => padding.Left + (innerWidth - contentSize.x) * 0.5f
        };

        var offsetY = vAlign switch {
            UIVerticalAlignment.Top => padding.Top,
            UIVerticalAlignment.Bottom => padding.Top + (innerHeight - contentSize.y),
            _ => padding.Top + (innerHeight - contentSize.y) * 0.5f
        };

        return new Vector3(offsetX, offsetY);
    }
    
    public static Vector2 CalculateAlignedOffset(UIHorizontalAlignment hAlign, UIVerticalAlignment vAlign, Vector2 availableSize, Vector2 contentSize) => CalculateAlignedOffset(hAlign, vAlign, availableSize, contentSize, EdgeInsets.Zero);

    public static float CalculateHorizontalOffset(UIHorizontalAlignment alignment, float availableWidth, float contentWidth, EdgeInsets padding) {
        var innerWidth = availableWidth - padding.Horizontal;
        return alignment switch {
            UIHorizontalAlignment.Left => padding.Left,
            UIHorizontalAlignment.Right => padding.Left + (innerWidth - contentWidth),
            _ => padding.Left + (innerWidth - contentWidth) * 0.5f
        };
    }

    public static float CalculateVerticalOffset(UIVerticalAlignment alignment, float availableHeight, float contentHeight, EdgeInsets padding) {
        var innerHeight = availableHeight - padding.Vertical;
        return alignment switch {
            UIVerticalAlignment.Top => padding.Top,
            UIVerticalAlignment.Bottom => padding.Top + (innerHeight - contentHeight),
            _ => padding.Top + (innerHeight - contentHeight) * 0.5f
        };
    }

    public static float CalculateHorizontalOffset(UIHorizontalAlignment alignment, float availableWidth, float contentWidth) => CalculateHorizontalOffset(alignment, availableWidth, contentWidth, EdgeInsets.Zero);

    public static float CalculateVerticalOffset(UIVerticalAlignment alignment, float availableHeight, float contentHeight) => CalculateVerticalOffset(alignment, availableHeight, contentHeight, EdgeInsets.Zero);
}
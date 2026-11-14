using UnityEngine;

namespace CSLModdingFramework.UI;

public readonly struct ColorStateSet {
    public readonly Color32 Disabled;
    public readonly Color32 Focused;
    public readonly Color32 Hovered;
    public readonly Color32 Normal;
    public readonly Color32 Pressed;

    public ColorStateSet(Color32 normal, Color32 hovered, Color32 pressed, Color32 focused, Color32 disabled) {
        Normal = normal;
        Hovered = hovered;
        Pressed = pressed;
        Focused = focused;
        Disabled = disabled;
    }
}
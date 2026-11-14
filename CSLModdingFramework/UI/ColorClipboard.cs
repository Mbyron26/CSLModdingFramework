using UnityEngine;

namespace CSLModdingFramework.UI;

public static class ColorClipboard {
    private static Color _color;

    public static bool HasValue { get; private set; }

    public static Color Color {
        get => _color;
        set {
            _color = value;
            HasValue = true;
        }
    }

    public static void Clear() => HasValue = false;
}
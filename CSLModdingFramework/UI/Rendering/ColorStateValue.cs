using ColossalFramework.UI;
using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public sealed class ColorStateValue : StateValue<Color32> {
    public ColorStateValue() => InitDefaultColors();

    public ColorStateValue(UIComponent parent) : base(parent) => InitDefaultColors();

    public ColorStateValue(UIComponent parent, Color32 normal, Color32 hovered, Color32 pressed, Color32 focused, Color32 disabled) : base(parent, normal, hovered, pressed, focused, disabled) { }

    protected override bool AreEqual(Color32 a, Color32 b) =>
        a.r == b.r &&
        a.g == b.g &&
        a.b == b.b &&
        a.a == b.a;

    public void SetValues(ColorStateSet set) => SetValues(set.Normal, set.Hovered, set.Pressed, set.Focused, set.Disabled);

    private void InitDefaultColors() {
        _normal = new Color32(255, 255, 255, 255);
        _hovered = new Color32(255, 255, 255, 255);
        _pressed = new Color32(255, 255, 255, 255);
        _focused = new Color32(255, 255, 255, 255);
        _disabled = new Color32(255, 255, 255, 255);
    }
}
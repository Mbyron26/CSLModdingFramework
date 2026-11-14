using ColossalFramework.UI;

namespace CSLModdingFramework.UI.Rendering;

public sealed class SpriteStateValue : StateValue<string> {
    public SpriteStateValue() { }
    public SpriteStateValue(UIComponent parent) : base(parent) { }
    public SpriteStateValue(UIComponent parent, string normal, string hovered, string pressed, string focused, string disabled) : base(parent, normal, hovered, pressed, focused, disabled) { }

    protected override bool AreEqual(string a, string b) => string.Equals(a, b);
}
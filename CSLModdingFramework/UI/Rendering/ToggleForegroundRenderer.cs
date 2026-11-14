using CSLModdingFramework.UI.Controls;
using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public sealed class ToggleForegroundRenderer : ForegroundRendererBase<Toggle> {
    public SpriteStateValue OffSprites { get; }
    public SpriteStateValue OnSprites { get; }
    public ColorStateValue OffColors { get; }
    public ColorStateValue OnColors { get; }

    public ToggleForegroundRenderer(Toggle owner) : base(owner) {
        OffSprites = new SpriteStateValue(owner);
        OnSprites = new SpriteStateValue(owner);
        OffColors = new ColorStateValue(owner);
        OnColors = new ColorStateValue(owner);
    }

    protected override string GetSprite() {
        var interactiveState = Owner.State;
        return Owner.IsOn ? OnSprites.GetValue(interactiveState) : OffSprites.GetValue(interactiveState);
    }

    protected override Color32 GetColor() {
        var interactiveState = Owner.State;
        return Owner.IsOn ? OnColors.GetValue(interactiveState) : OffColors.GetValue(interactiveState);
    }
}
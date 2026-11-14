using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public sealed class SelectableBackgroundRenderer<T> : BackgroundRendererBase<T> where T : UIInteractiveControl, ISelectable {
    public SpriteStateValue UnselectedSprites { get; }
    public SpriteStateValue SelectedSprites { get; }
    public ColorStateValue UnselectedColors { get; }
    public ColorStateValue SelectedColors { get; }

    public SelectableBackgroundRenderer(T owner) : base(owner) {
        UnselectedSprites = new SpriteStateValue(owner);
        SelectedSprites = new SpriteStateValue(owner);
        UnselectedColors = new ColorStateValue(owner);
        SelectedColors = new ColorStateValue(owner);
    }

    protected override string GetSprite() {
        var interactiveState = Owner.State;
        return Owner.IsSelected ? SelectedSprites.GetValue(interactiveState) : UnselectedSprites.GetValue(interactiveState);
    }

    protected override Color32 GetColor() {
        var interactiveState = Owner.State;
        return Owner.IsSelected ? SelectedColors.GetValue(interactiveState) : UnselectedColors.GetValue(interactiveState);
    }
}
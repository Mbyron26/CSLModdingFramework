using UnityEngine;

namespace CSLModdingFramework.UI.Rendering;

public class BackgroundRenderer : BackgroundRendererBase<UIInteractiveControl> {
    public SpriteStateValue Sprites { get; }
    public ColorStateValue Colors { get; }

    public BackgroundRenderer(UIInteractiveControl owner) : base(owner) {
        Sprites = new SpriteStateValue(Owner);
        Colors = new ColorStateValue(Owner);
    }

    protected override string GetSprite() => Sprites.GetValue(Owner.State);
    protected override Color32 GetColor() => Colors.GetValue(Owner.State);
}
using CSLModdingFramework.UI.Rendering;

namespace CSLModdingFramework.UI.Controls;

public class Container : UIControl {
    protected LiteBackgroundRenderer _background;
    protected LiteForegroundRenderer _foreground;

    public LiteBackgroundRenderer Background => _background ??= new LiteBackgroundRenderer(this);
    public LiteForegroundRenderer Foreground => _foreground ??= new LiteForegroundRenderer(this);
    
    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
    }
}
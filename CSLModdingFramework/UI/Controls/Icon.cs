using CSLModdingFramework.UI.Rendering;

namespace CSLModdingFramework.UI.Controls;

public class Icon : UIInteractiveControl {
    protected BackgroundRenderer _background;
    protected ForegroundRenderer _foreground;

    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);
    public ForegroundRenderer Foreground => _foreground ??= new ForegroundRenderer(this);

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
    }
}
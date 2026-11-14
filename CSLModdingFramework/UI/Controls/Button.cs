using System;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;

namespace CSLModdingFramework.UI.Controls;

public class Button : TextControl {
    protected BackgroundRenderer _background;
    protected ForegroundRenderer _foreground;

    public event Action Clicked;

    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);
    public ForegroundRenderer Foreground => _foreground ??= new ForegroundRenderer(this);

    public override void Awake() {
        base.Awake();

        TextColors.SetValues(UIColorPalette.HeaderTextColors);
    }

    protected override void OnClick(UIMouseEventParameter p) {
        Clicked?.Invoke();

        base.OnClick(p);
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
        RenderText();
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended)
            PerformAutoLayout();

        base.OnSizeChanged();
    }

    public void ApplySettingItemControlStyle(ThemeKind theme) {
        Background.Atlas = DefaultAtlas.Instance;
        Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
        Background.Colors.SetValues(theme == ThemeKind.ClassicLight ? UIColorPalette.ClassicLightPalette.SettingItemControlColors : UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
    }

    public void ApplySettingsSectionStyle(ThemeKind theme) {
        Background.Atlas = DefaultAtlas.Instance;
        Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
        Background.Colors.SetValues(theme == ThemeKind.ClassicLight ? UIColorPalette.ClassicLightPalette.SettingItemControlColors : UIColorPalette.ClassicDarkPalette.SettingsSectionColors);
    }

    public void ApplyStyle(ThemeKind style) {
        Background.Atlas = DefaultAtlas.Instance;
        Background.Sprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);

        Background.Colors.SetValues(style == ThemeKind.ClassicLight ? UIColorPalette.ClassicLightPalette.SettingItemControlColors : UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
    }
}
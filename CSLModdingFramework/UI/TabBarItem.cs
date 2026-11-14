using CSLModdingFramework.UI.Rendering;

namespace CSLModdingFramework.UI;

public class TabBarItem : TextControl, ISelectable, IThemeable {
    protected bool _isSelected;
    protected ThemeKind _theme;
    protected SelectableBackgroundRenderer<TabBarItem> _background;
    protected SelectableForegroundRenderer<TabBarItem> _foreground;

    public event UIEventHandler<TabBarItem, bool> SelectionChanged;

    public SelectableBackgroundRenderer<TabBarItem> Background => _background ??= new SelectableBackgroundRenderer<TabBarItem>(this);

    public SelectableForegroundRenderer<TabBarItem> Foreground => _foreground ??= new SelectableForegroundRenderer<TabBarItem>(this);

    public virtual bool IsSelected {
        get => _isSelected;
        set => SetSelection(value);
    }

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public int Index { get; internal set; }

    public virtual void SetTheme(ThemeKind value) {
        if (value == _theme) return;

        _theme = value;
        var backgroundRenderer = Background;
        backgroundRenderer.Atlas = DefaultAtlas.Instance;
        backgroundRenderer.SelectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
        backgroundRenderer.UnselectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
        backgroundRenderer.UnselectedSprites.Normal = null;

        if (value == ThemeKind.ClassicLight) {
            backgroundRenderer.SelectedColors.SetValues(UIColorPalette.GreenColors);
            backgroundRenderer.UnselectedColors.SetValues(UIColorPalette.ClassicLightPalette.SettingItemControlColors);
        }
        else {
            backgroundRenderer.SelectedColors.SetValues(UIColorPalette.BlueColors);
            backgroundRenderer.UnselectedColors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
        }
    }

    public virtual bool SetSelection(bool newState, bool notify = true) {
        if (_isSelected == newState) return false;

        _isSelected = newState;
        if (notify)
            NotifySelectionChanged(newState);

        Invalidate();
        return true;
    }

    protected void NotifySelectionChanged(bool newValue) => SelectionChanged?.Invoke(this, newValue);

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _foreground?.Render();
        RenderText();
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended) {
            PerformAutoLayout();
        }

        base.OnSizeChanged();
    }
}
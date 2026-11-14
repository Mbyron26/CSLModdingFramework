using CSLModdingFramework.Lib.Numeric;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public class Checkbox : SelectionControl, IThemeable {
    protected BackgroundRenderer _background;
    protected CheckState _checkState;
    protected CheckBoxIndicatorRenderer _indicatorRenderer;
    protected ThemeKind _theme;

    public event UIEventHandler<Checkbox, ValueChangedArgs<CheckState>> CheckStateChanged;

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public BackgroundRenderer Background => _background ??= new BackgroundRenderer(this);

    public CheckState CheckState {
        get => _checkState;
        set => SetCheckState(value);
    }

    public int Id { get; internal set; }

    public CheckBoxIndicatorRenderer IndicatorRenderer => _indicatorRenderer ??= new CheckBoxIndicatorRenderer(this);

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(100, 20);
        _indicatorSize = new Vector2(20, 20);
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        _indicatorRenderer?.Render();
        RenderText();
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended) {
            PerformAutoLayout();
        }

        base.OnSizeChanged();
    }

    public bool SetCheckState(CheckState newState, bool notify = true) {
        if (!isEnabled || newState == _checkState) return false;

        var oldValue = _checkState;
        _checkState = newState;

        Invalidate();

        if (notify)
            OnCheckStateChanged(oldValue, newState);

        return true;
    }

    public bool SetCheckStateWithoutNotify(CheckState newState) => SetCheckState(newState, false);

    public void SetTheme(ThemeKind theme) {
        if (_theme == theme) return;

        _theme = theme;
        var renderer = IndicatorRenderer;

        renderer.Atlas = DefaultAtlas.Instance;
        renderer.ForegroundCustomSize = _indicatorSize;
        renderer.ForegroundScale = 0.6f;

        renderer.BackgroundSprite = DefaultAtlas.SmoothRectangleRadius4;
        renderer.ForegroundCheckedSprite = DefaultAtlas.Checked;
        renderer.ForegroundIndeterminateSprite = DefaultAtlas.IndeterminateIcon;
        renderer.ForegroundCheckedColors.SetValues(UIColorPalette.HeaderTextColors);
        renderer.ForegroundIndeterminateColors.SetValues(UIColorPalette.HeaderTextColors);
        renderer.ForegroundUncheckedColors.SetValues(UIColorPalette.HeaderTextColors);

        switch (theme) {
            case ThemeKind.ClassicLight:
                renderer.BackgroundCheckedColors.SetValues(UIColorPalette.GreenColors);
                renderer.BackgroundIndeterminateColors.SetValues(UIColorPalette.GreenColors);
                renderer.BackgroundUncheckedColors.SetValues(UIColorPalette.ClassicLightPalette.SettingItemControlColors);
                break;

            case ThemeKind.ClassicDark:
            default:
                renderer.BackgroundCheckedColors.SetValues(UIColorPalette.BlueColors);
                renderer.BackgroundIndeterminateColors.SetValues(UIColorPalette.BlueColors);
                renderer.BackgroundUncheckedColors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
                break;
        }
    }

    private void OnCheckStateChanged(CheckState oldValue, CheckState newState) => CheckStateChanged?.Invoke(this, new ValueChangedArgs<CheckState> {
        OldValue = oldValue,
        NewValue = newState
    });
}
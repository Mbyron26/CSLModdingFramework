using System;
using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public class RadioButton : SelectionControl, IThemeable {
    protected UIRenderData _indicatorRenderData;
    protected IAtlas _atlas;
    protected bool _isSelected;
    protected ThemeKind _theme;

    public event UIEventHandler<RadioButton, bool> SelectionChanged;
    public event Action<bool> SelectionValueChanged;

    public SpriteStateValue IndicatorUnselectedSprites { get; private set; }
    public SpriteStateValue IndicatorSelectedSprites { get; private set; }
    public ColorStateValue IndicatorUnselectedColors { get; private set; }
    public ColorStateValue IndicatorSelectedColors { get; private set; }
    public int Id { get; internal set; }

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public IAtlas Atlas {
        get => _atlas;
        set {
            if (Equals(value, _atlas)) return;

            _atlas = value;
            Invalidate();
        }
    }

    public bool Selection {
        get => _isSelected;
        set => SetSelection(value);
    }

    public override void Awake() {
        base.Awake();

        m_Size = new Vector2(100, 20);
        _indicatorSize = new Vector2(20, 20);
        IndicatorUnselectedSprites = new SpriteStateValue(this);
        IndicatorSelectedSprites = new SpriteStateValue(this);
        IndicatorUnselectedColors = new ColorStateValue(this);
        IndicatorSelectedColors = new ColorStateValue(this);
    }

    protected override void OnSizeChanged() {
        if (!isLayoutSuspended) {
            PerformAutoLayout();
        }

        base.OnSizeChanged();
    }

    protected override void OnRebuildRenderData() {
        if (IsAutoLayoutSuspended) return;

        RenderIndicator();
        RenderText();
    }

    public bool SetSelectionWithoutNotify(bool newState) => SetSelection(newState, false);

    public bool SetSelection(bool newState, bool notify = true) {
        if (!isEnabled || newState == _isSelected) return false;

        _isSelected = newState;

        Invalidate();

        if (notify) {
            NotifySelectionChanged(_isSelected);
            NotifySelectionValueChanged(_isSelected);
        }

        return true;
    }

    public virtual void SetTheme(ThemeKind theme) {
        if (_theme == theme) return;

        _theme = theme;
        Atlas = DefaultAtlas.Instance;
        IndicatorUnselectedSprites.SetValues(DefaultAtlas.Circle);
        IndicatorSelectedSprites.SetValues(DefaultAtlas.RadioButtonOn);

        switch (theme) {
            case ThemeKind.ClassicLight:
                IndicatorUnselectedColors.SetValues(UIColorPalette.ClassicLightPalette.SettingItemControlColors);
                IndicatorSelectedColors.SetValues(UIColorPalette.GreenColors);
                break;
            case ThemeKind.ClassicDark:
            default:
                IndicatorUnselectedColors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
                IndicatorSelectedColors.SetValues(UIColorPalette.BlueColors);
                break;
        }
    }

    private void RenderIndicator() {
        if (_atlas == null) return;

        if (_indicatorRenderData is not null) {
            _indicatorRenderData.Clear();
        }
        else {
            _indicatorRenderData = UIRenderData.Obtain();
            RenderData.Add(_indicatorRenderData);
        }

        _indicatorRenderData.material = _atlas.Material;

        var spriteName = _isSelected ? IndicatorSelectedSprites.GetValue(_state) : IndicatorUnselectedSprites.GetValue(_state);

        if (!_atlas.TryGetSprite(spriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _atlas.GetSprite(spriteIndex);
        
        var renderColor = _isSelected ? IndicatorSelectedColors.GetValue(_state) : IndicatorUnselectedColors.GetValue(_state);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _atlas,
            Color = renderColor,
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = new Vector3(_layoutPadding.Left, -_layoutPadding.Top, 0),
            UnitsPerPixel = UnitsPerPixel,
            Size = _indicatorSize,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_indicatorRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_indicatorRenderData, spriteRenderData);
    }

    protected void NotifySelectionChanged(bool value) => SelectionChanged?.Invoke(this, value);
    protected void NotifySelectionValueChanged(bool value) => SelectionValueChanged?.Invoke(value);
}
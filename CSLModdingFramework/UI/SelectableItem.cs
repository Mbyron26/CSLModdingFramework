using ColossalFramework.UI;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public class SelectableItem : TextControl, IThemeable {
    protected IAtlas _backgroundAtlas;
    protected IAtlas _iconAtlas;
    protected Vector2 _iconOffset = new(10, 0);
    protected UIRenderData _iconRenderData;
    protected float _iconScale = 1f;
    protected Vector2 _iconSize = new(16, 16);
    protected string _iconSpriteName = string.Empty;
    protected float _iconTextGap = 10f;
    protected bool _isSelected;
    protected ThemeKind _theme;

    public event UIEventHandler<SelectableItem, bool> SelectionChanged;

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public override TextSizeMode SizeMode => TextSizeMode.Fixed;
    public override bool TextWordWrap => _textWordWrap;

    public IAtlas BackgroundAtlas {
        get => _backgroundAtlas;
        set {
            if (ReferenceEquals(value, _backgroundAtlas)) return;

            _backgroundAtlas = value;
            Invalidate();
        }
    }

    public ColorStateValue BackgroundSelectedColors { get; private set; }
    public SpriteStateValue BackgroundSelectedSprites { get; private set; }
    public ColorStateValue BackgroundUnselectedColors { get; private set; }
    public SpriteStateValue BackgroundUnselectedSprites { get; private set; }
    public Anchor IconAnchor => Anchor.MiddleLeft;

    public IAtlas IconAtlas {
        get => _iconAtlas;
        set {
            if (Equals(value, _iconAtlas)) return;

            _iconAtlas = value;
            Invalidate();
        }
    }

    public Vector2 IconOffset {
        get => _iconOffset;
        set {
            var clamped = new Vector2(Mathf.Max(0f, value.x), value.y);
            if (clamped == _iconOffset) return;

            _iconOffset = clamped;
            InvalidateAutoLayout();
        }
    }

    public float IconScale {
        get => _iconScale;
        set {
            if (Mathf.Approximately(value, _iconScale)) return;

            _iconScale = value;
            InvalidateAutoLayout();
        }
    }

    public ColorStateValue IconSelectedColors { get; private set; }

    public Vector2 IconSize {
        get => _iconSize;
        set {
            if (value.Equals(_iconSize)) return;

            _iconSize = value;
            InvalidateAutoLayout();
        }
    }

    public string IconSpriteName {
        get => _iconSpriteName;
        set {
            if (string.Equals(_iconSpriteName, value)) return;

            _iconSpriteName = value;
            Invalidate();
        }
    }

    public float IconTextGap {
        get => _iconTextGap;
        set {
            if (value.Equals(_iconTextGap)) return;

            _iconTextGap = value;
            InvalidateAutoLayout();
        }
    }

    public ColorStateValue IconUnselectedColors { get; private set; }
    public int Id { get; internal set; }

    public bool IsSelected {
        get => _isSelected;
        set => SetSelection(value);
    }

    public virtual bool SetSelection(bool newState, bool notify = true) {
        if (_isSelected == newState) return false;

        _isSelected = newState;

        if (notify)
            OnSelectionChanged(newState);

        Invalidate();
        return true;
    }

    public virtual bool SetSelectionWithoutNotify(bool newState) => SetSelection(newState, false);

    public virtual void SetTheme(ThemeKind style) {
        if (_theme == style) return;

        _theme = style;
        _backgroundAtlas = DefaultAtlas.Instance;
        BackgroundUnselectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
        BackgroundUnselectedSprites.Normal = string.Empty;
        BackgroundSelectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);

        if (style == ThemeKind.ClassicDark) {
            BackgroundUnselectedColors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
            BackgroundSelectedColors.SetValues(UIColorPalette.BlueNormal);
        }
        else {
            BackgroundUnselectedColors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
            BackgroundSelectedColors.SetValues(UIColorPalette.GreenNormal);
        }
    }

    protected virtual void OnSelectionChanged(bool newValue) => NotifySelectionChanged(newValue);

    protected void NotifySelectionChanged(bool newValue) => SelectionChanged?.Invoke(this, newValue);

    protected virtual void RenderBackground() {
        if (_backgroundAtlas == null) return;

        renderData.material = _backgroundAtlas.Material;
        var renderSpriteName = _isSelected ? BackgroundSelectedSprites.GetValue(_state) : BackgroundUnselectedSprites.GetValue(_state);

        if (!_backgroundAtlas.TryGetSprite(renderSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _backgroundAtlas.GetSprite(spriteIndex);
        var renderColor = _isSelected ? BackgroundSelectedColors.GetValue(_state) : BackgroundUnselectedColors.GetValue(_state);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _backgroundAtlas,
            Color = renderColor,
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = Vector2.zero,
            UnitsPerPixel = UnitsPerPixel,
            Size = size,
            SpriteMetadata = spriteMetadata
        };
        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(renderData, spriteRenderData);
    }

    protected virtual void RenderIcon() {
        if (_iconAtlas == null) return;

        if (_iconRenderData is not null) {
            _iconRenderData.Clear();
        }
        else {
            _iconRenderData = UIRenderData.Obtain();
            RenderData.Add(_iconRenderData);
        }

        if (string.IsNullOrEmpty(_iconSpriteName)) return;

        _iconRenderData.material = _iconAtlas.Material;

        if (!_iconAtlas.TryGetSprite(_iconSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _iconAtlas.GetSprite(spriteIndex);
        var renderColor = _isSelected ? IconSelectedColors.GetValue(_state) : IconUnselectedColors.GetValue(_state);
        var iconSize = _iconSize * _iconScale;
        var offset = (IconAnchor.CalculateTopLeftAnchorOffset(iconSize, size) + _iconOffset).Scale(1, -1);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _iconAtlas,
            Color = renderColor,
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = offset,
            UnitsPerPixel = UnitsPerPixel,
            Size = iconSize,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_iconRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_iconRenderData, spriteRenderData);
    }

    public override void Awake() {
        base.Awake();

        _textHorizontalAlignment = UIHorizontalAlignment.Left;
        BackgroundUnselectedSprites = new SpriteStateValue(this);
        BackgroundSelectedSprites = new SpriteStateValue(this);
        BackgroundUnselectedColors = new ColorStateValue(this);
        BackgroundSelectedColors = new ColorStateValue(this);
        IconUnselectedColors = new ColorStateValue(this);
        IconSelectedColors = new ColorStateValue(this);
    }

    public override void PerformAutoLayout() {
        var fullWidth = m_Size.x;
        var fullHeight = m_Size.y;
        var contentHeight = fullHeight - _textPadding.Vertical;

        var hasIcon = !string.IsNullOrEmpty(_iconSpriteName);
        float textRegionX;

        if (hasIcon) {
            var iconSize = _iconSize * _iconScale;
            textRegionX = _textPadding.Left + _iconOffset.x + iconSize.x + _iconTextGap;
        }
        else {
            textRegionX = _textPadding.Left;
        }

        _textRenderMaxWidth = fullWidth - textRegionX - _textPadding.Right;
        _textRenderMaxHeight = contentHeight;

        var textIntrinsicSize = MeasureRenderedTextSize();
        var shouldShowTooltip = textIntrinsicSize.x > _textRenderMaxWidth;

        float finalTextOffsetX;

        if (shouldShowTooltip) {
            finalTextOffsetX = textRegionX;
        }
        else {
            switch (_textHorizontalAlignment) {
                case UIHorizontalAlignment.Left:
                    finalTextOffsetX = textRegionX;
                    break;

                case UIHorizontalAlignment.Right:
                    finalTextOffsetX = textRegionX + (_textRenderMaxWidth - textIntrinsicSize.x);
                    break;

                case UIHorizontalAlignment.Center:
                default:
                    finalTextOffsetX = textRegionX + (_textRenderMaxWidth - textIntrinsicSize.x) * 0.5f;
                    break;
            }
        }

        float finalTextOffsetY;

        switch (_textVerticalAlignment) {
            case UIVerticalAlignment.Top:
                finalTextOffsetY = _textPadding.Top;
                break;

            case UIVerticalAlignment.Bottom:
                finalTextOffsetY = fullHeight - _textPadding.Bottom - textIntrinsicSize.y;
                break;

            case UIVerticalAlignment.Middle:
            default:
                finalTextOffsetY = _textPadding.Top + (contentHeight - textIntrinsicSize.y) * 0.5f;
                break;
        }

        _textRenderPosition = new Vector2(finalTextOffsetX, finalTextOffsetY);

        if (tooltipBox != null)
            tooltip = shouldShowTooltip ? _text : null;
    }

    protected override void OnRebuildRenderData() {
        if (IsAutoLayoutSuspended) return;

        RenderBackground();
        RenderIcon();
        RenderText();
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended)
            PerformAutoLayout();

        base.OnSizeChanged();
    }

    public float MeasureRequiredWidth() {
        var result = MeasureRenderedTextSize().x;
        result += _textPadding.Horizontal;
        if (string.IsNullOrEmpty(_iconSpriteName)) return result;

        var iconSize = _iconSize * _iconScale;
        result += _iconOffset.x + iconSize.x + _iconTextGap;
        return result;
    }
}
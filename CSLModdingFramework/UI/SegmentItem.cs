using ColossalFramework.UI;
using CSLModdingFramework.UI.Fonts;
using CSLModdingFramework.UI.Rendering;
using UnityEngine;

namespace CSLModdingFramework.UI;

public class SegmentItem : UIInteractiveControl, ISelectable, IThemeable, IAutoLayout {
    protected int _autoLayoutSuspendCount;
    protected SelectableBackgroundRenderer<SegmentItem> _background;
    protected SegmentItemContentKind _contentKind;
    protected Orientation _direction;
    protected DynamicFont _font;
    protected UIHorizontalAlignment _horizontalAlignment = UIHorizontalAlignment.Center;
    protected IAtlas _iconAtlas;
    protected Color32 _iconDisabledColor = UIColorPalette.White;
    protected Color32 _iconNormalColor = UIColorPalette.White;
    protected string _iconSpriteName;
    protected UIRenderData _iconRenderData;
    protected Vector3 _iconRenderPosition;
    protected Vector2 _iconRenderSize;
    protected Vector2 _iconSize = new(18, 18);
    protected float _iconTextSpacing = 6f;
    protected bool _iconVisible;
    protected bool _isSelected;
    protected EdgeInsets _padding;
    protected string _text;
    protected Vector2 _textAvailableSize;
    protected float _textCharacterSpacing = 1f;
    protected float _textLineSpacing;
    protected bool _textProcessMarkup;
    protected UIRenderData _textRenderData;
    protected Vector3 _textRenderPosition;
    protected float _textScale = 1f;
    protected float _textSpaceOffset;
    protected int _textTabSize = 48;
    protected bool _textVisible;
    protected ThemeKind _theme;
    protected UIVerticalAlignment _verticalAlignment = UIVerticalAlignment.Middle;

    public event UIEventHandler<SegmentItem, bool> SelectionChanged;

    public virtual SegmentItemContentKind ContentKind {
        get => _contentKind;
        set {
            if (value == _contentKind) return;

            _contentKind = value;
            InvalidateAutoLayout();
        }
    }

    public virtual Orientation Direction {
        get => _direction;
        set {
            if (value == _direction) return;

            _direction = value;
            InvalidateAutoLayout();
        }
    }

    public virtual UIHorizontalAlignment HorizontalAlignment {
        get => _horizontalAlignment;
        set {
            if (value == _horizontalAlignment) return;

            _horizontalAlignment = value;
            InvalidateAutoLayout();
        }
    }

    public virtual IAtlas IconAtlas {
        get => _iconAtlas;
        set {
            if (Equals(value, _iconAtlas)) return;

            _iconAtlas = value;
            Invalidate();
        }
    }

    public virtual Color32 IconDisabledColor {
        get => _iconDisabledColor;
        set {
            if (_iconDisabledColor.EqualsColor(value)) return;

            _iconDisabledColor = value;
            Invalidate();
        }
    }

    public virtual Color32 IconNormalColor {
        get => _iconNormalColor;
        set {
            if (_iconNormalColor.EqualsColor(value)) return;

            _iconNormalColor = value;
            Invalidate();
        }
    }

    public virtual string IconSpriteName {
        get => _iconSpriteName;
        set {
            if (string.Equals(_iconSpriteName, value)) return;

            _iconSpriteName = value;
            Invalidate();
        }
    }

    public virtual Vector2 IconSize {
        get => _iconSize;
        set {
            if (_iconSize == value) return;

            _iconSize = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float IconTextSpacing {
        get => _iconTextSpacing;
        set {
            if (Mathf.Approximately(_iconTextSpacing, value)) return;

            _iconTextSpacing = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool IsSelected {
        get => _isSelected;
        set => SetSelected(value);
    }

    public virtual EdgeInsets Padding {
        get => _padding;
        set {
            if (value.EqualsApprox(_padding)) return;

            _padding = value;
            InvalidateAutoLayout();
        }
    }

    public virtual string Text {
        get => _text;
        set {
            if (string.Equals(value, _text)) return;

            _text = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float TextCharacterSpacing {
        get => _textCharacterSpacing;
        set {
            value = Mathf.Max(0, value);
            if (Mathf.Approximately(value, _textCharacterSpacing)) return;

            _textCharacterSpacing = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float TextLineSpacing {
        get => _textLineSpacing;
        set {
            if (Mathf.Approximately(_textLineSpacing, value)) return;

            _textLineSpacing = value;
            InvalidateAutoLayout();
        }
    }

    public virtual bool TextProcessMarkup {
        get => _textProcessMarkup;
        set {
            if (value == _textProcessMarkup) return;

            _textProcessMarkup = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float TextScale {
        get => _textScale;
        set {
            value = Mathf.Max(0.1f, value);
            if (Mathf.Approximately(_textScale, value)) return;

            _textScale = value;
            InvalidateAutoLayout();
        }
    }

    public virtual float TextSpaceOffset {
        get => _textSpaceOffset;
        set {
            if (Mathf.Approximately(_textSpaceOffset, value)) return;

            _textSpaceOffset = value;
            InvalidateAutoLayout();
        }
    }

    public virtual int TextTabSize {
        get => _textTabSize;
        set {
            value = Mathf.Max(0, value);
            if (value == _textTabSize) return;

            _textTabSize = value;
            InvalidateAutoLayout();
        }
    }

    public virtual ThemeKind Theme {
        get => _theme;
        set => SetTheme(value);
    }

    public virtual UIVerticalAlignment VerticalAlignment {
        get => _verticalAlignment;
        set {
            if (value == _verticalAlignment) return;

            _verticalAlignment = value;
            InvalidateAutoLayout();
        }
    }

    public SelectableBackgroundRenderer<SegmentItem> Background => _background ??= new SelectableBackgroundRenderer<SegmentItem>(this);
    public DynamicFont Font => _font ??= FontManager.OpenSansRegularFont;
    public int Id { get; internal set; }
    public bool IsAutoLayoutSuspended => _autoLayoutSuspendCount > 0;
    public bool IsPerformingAutoLayout { get; protected set; }

    public ColorStateValue TextColors { get; private set; }

    public virtual void PerformAutoLayout() {
        if (IsPerformingAutoLayout) return;

        IsPerformingAutoLayout = true;
        _iconVisible = false;
        _textVisible = false;
        switch (_contentKind) {
            case SegmentItemContentKind.Icon:
                PerformIconLayout();
                break;

            case SegmentItemContentKind.Text:
                PerformTextLayout();
                break;

            case SegmentItemContentKind.IconText:
                PerformCombinedLayout();
                break;
            default:
                PerformIconLayout();
                break;
        }

        IsPerformingAutoLayout = false;
    }

    public virtual DynamicFontRenderer RentTextRenderer() {
        var fontRenderer = Font.RentRenderer();
        fontRenderer.CharacterSpacing = _textCharacterSpacing;
        fontRenderer.WordWrap = false;
        fontRenderer.LineSpacing = _textLineSpacing;
        fontRenderer.SpaceAdditionalWidth = _textSpaceOffset;
        fontRenderer.MaxSize = _textAvailableSize;
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _textScale;
        fontRenderer.LayoutOffset = _textRenderPosition;
        fontRenderer.TextAlign = _horizontalAlignment;
        fontRenderer.ProcessMarkup = _textProcessMarkup;
        fontRenderer.DefaultColor = GetTextColor();
        fontRenderer.Opacity = 1f;
        return fontRenderer;
    }

    public virtual void ResumeAutoLayout(bool performNow = true) {
        if (_autoLayoutSuspendCount == 0) return;

        _autoLayoutSuspendCount--;

        if (_autoLayoutSuspendCount != 0 || !performNow) return;

        PerformAutoLayout();
        Invalidate();
    }

    public virtual bool SetSelected(bool newState, bool notify = true) {
        if (_isSelected == newState) return false;

        _isSelected = newState;
        Invalidate();

        if (notify)
            NotifySelectionChanged(newState);
        return true;
    }

    public virtual bool SetSelectedWithoutNotify(bool newState) => SetSelected(newState, false);

    public virtual void SetTheme(ThemeKind theme) {
        if (_theme == theme) return;

        _theme = theme;
        var backgroundRender = Background;
        backgroundRender.Atlas = DefaultAtlas.Instance;

        if (theme == ThemeKind.ClassicLight) {
            backgroundRender.SelectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
            backgroundRender.UnselectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
            backgroundRender.UnselectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius6);
            backgroundRender.UnselectedSprites.Normal = string.Empty;


            backgroundRender.SelectedColors.SetValues(UIColorPalette.GreenColors);
            backgroundRender.UnselectedColors.SetValues(UIColorPalette.ClassicLightPalette.SettingItemControlColors);
        }
        else {
            backgroundRender.SelectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
            backgroundRender.UnselectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
            backgroundRender.UnselectedSprites.SetValues(DefaultAtlas.SmoothRectangleRadius8);
            backgroundRender.UnselectedSprites.Normal = string.Empty;

            backgroundRender.SelectedColors.SetValues(UIColorPalette.BlueColors);
            backgroundRender.UnselectedColors.SetValues(UIColorPalette.ClassicDarkPalette.SettingItemControlColors);
        }
    }

    public virtual void SuspendAutoLayout() => _autoLayoutSuspendCount++;

    protected virtual Color32 GetTextColor() => InvalidateOnStateChanged ? TextColors.GetValue(_state) : isEnabled ? TextColors.Normal : TextColors.Disabled;

    protected virtual Vector2 GetTextRenderedSize(Vector2 maxSize) {
        if (string.IsNullOrEmpty(_text)) return Vector2.zero;

        using var fontRenderer = Font.RentRenderer();
        fontRenderer.CharacterSpacing = _textCharacterSpacing;
        fontRenderer.WordWrap = false;
        fontRenderer.LineSpacing = _textLineSpacing;
        fontRenderer.SpaceAdditionalWidth = _textSpaceOffset;
        fontRenderer.MaxSize = maxSize;
        fontRenderer.PixelRatio = UnitsPerPixel;
        fontRenderer.TextScale = _textScale;
        fontRenderer.TextAlign = UIHorizontalAlignment.Left;
        fontRenderer.ProcessMarkup = _textProcessMarkup;
        return fontRenderer.GetRenderedSize(_text);
    }

    protected virtual void InvalidateAutoLayout() {
        if (IsAutoLayoutSuspended) return;

        PerformAutoLayout();
        Invalidate();
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

        if (!_iconVisible) return;

        _iconRenderData.material = _iconAtlas.Material;

        if (!_iconAtlas.TryGetSprite(_iconSpriteName, out int spriteIndex)) return;

        ref readonly var spriteMetadata = ref _iconAtlas.GetSprite(spriteIndex);

        SpriteRenderData spriteRenderData = new() {
            Atlas = _iconAtlas,
            Color = isEnabled ? _iconNormalColor : _iconDisabledColor,
            FillAmount = 1f,
            Flip = UISpriteFlip.None,
            Offset = _iconRenderPosition,
            UnitsPerPixel = UnitsPerPixel,
            Size = _iconRenderSize,
            SpriteMetadata = spriteMetadata
        };

        if (spriteMetadata.IsSliced) {
            SlicedSpriteMeshBuilder.RenderSprite(_iconRenderData, spriteRenderData);
            return;
        }

        SpriteMeshBuilder.RenderSprite(_iconRenderData, spriteRenderData);
    }

    protected virtual void RenderText() {
        if (Font is not { IsValid: true }) return;

        if (_textRenderData != null) {
            _textRenderData.Clear();
        }
        else {
            _textRenderData = UIRenderData.Obtain();
            m_RenderData.Add(_textRenderData);
        }

        if (_contentKind == SegmentItemContentKind.Icon || string.IsNullOrEmpty(_text)) return;
        if (!_textVisible) return;

        using var uiFontRenderer = RentTextRenderer();
        uiFontRenderer.Render(_text, _textRenderData);
    }

    public override void Awake() {
        base.Awake();

        TextColors = new ColorStateValue(this);
        m_Size = new Vector2(32, 32);
    }

    public override void OnDisable() {
        base.OnDisable();

        UnityEngine.Font.textureRebuilt -= OnUnityFontTextureRebuilt;
    }

    public override void OnEnable() {
        base.OnEnable();

        UnityEngine.Font.textureRebuilt += OnUnityFontTextureRebuilt;
    }

    protected override void OnRebuildRenderData() {
        _background?.Render();
        RenderText();
        RenderIcon();
    }

    protected override void OnSizeChanged() {
        if (!IsAutoLayoutSuspended)
            PerformAutoLayout();

        base.OnSizeChanged();
    }

    protected Vector3 GetAlignedPosition(Vector2 contentSize, Vector2 elementSize) {
        var x = _horizontalAlignment switch {
            UIHorizontalAlignment.Left => _padding.Left,
            UIHorizontalAlignment.Center => _padding.Left + (contentSize.x - elementSize.x) * 0.5f,
            UIHorizontalAlignment.Right => size.x - _padding.Right - elementSize.x,
            _ => 0
        };

        var y = _verticalAlignment switch {
            UIVerticalAlignment.Top => _padding.Top,
            UIVerticalAlignment.Middle => _padding.Top + (contentSize.y - elementSize.y) * 0.5f,
            UIVerticalAlignment.Bottom => size.y - _padding.Bottom - elementSize.y,
            _ => 0
        };

        return new Vector3(x, y, 0);
    }

    protected Vector2 GetContentSize() => new(
        Mathf.Max(0, size.x - _padding.Left - _padding.Right),
        Mathf.Max(0, size.y - _padding.Top - _padding.Bottom)
    );

    protected void NotifySelectionChanged(bool newValue) => SelectionChanged?.Invoke(this, newValue);

    protected void PerformCombinedLayout() {
        switch (_direction) {
            case Orientation.Horizontal:
                PerformHorizontalCombinedLayout();
                break;

            case Orientation.Vertical:
                PerformVerticalCombinedLayout();
                break;
        }
    }

    protected void PerformHorizontalCombinedLayout() {
        var contentSize = GetContentSize();

        var iconSize = Vector2.Min(_iconSize, contentSize);
        var textAvailableWidth = contentSize.x - iconSize.x - _iconTextSpacing;

        if (textAvailableWidth <= 0) return;

        var textSize = GetTextRenderedSize(new Vector2(textAvailableWidth, contentSize.y));

        var groupSize = new Vector2(
            iconSize.x + _iconTextSpacing + textSize.x,
            Mathf.Max(iconSize.y, textSize.y)
        );

        if (groupSize.x > contentSize.x || groupSize.y > contentSize.y) return;

        var groupPosition = GetAlignedPosition(contentSize, groupSize);

        _iconRenderSize = iconSize;
        _textAvailableSize = textSize;

        _iconRenderPosition = new Vector3(
            groupPosition.x,
            groupPosition.y + (groupSize.y - iconSize.y) * 0.5f
        );

        _textRenderPosition = new Vector3(
            groupPosition.x + iconSize.x + _iconTextSpacing,
            groupPosition.y + (groupSize.y - textSize.y) * 0.5f
        );

        _iconVisible = true;
        _textVisible = true;
    }

    protected void PerformIconLayout() {
        var contentSize = GetContentSize();
        _iconRenderSize = Vector2.Min(_iconSize, contentSize);
        _iconRenderPosition = GetAlignedPosition(contentSize, _iconRenderSize);
        _iconVisible = true;
    }

    protected void PerformTextLayout() {
        var contentSize = GetContentSize();
        _textAvailableSize = Vector2.Min(GetTextRenderedSize(contentSize), contentSize);
        _textRenderPosition = GetAlignedPosition(contentSize, _textAvailableSize);
        _textVisible = true;
    }

    protected void PerformVerticalCombinedLayout() {
        var contentSize = GetContentSize();

        var iconSize = Vector2.Min(
            _iconSize,
            contentSize
        );

        var textAvailableHeight = contentSize.y - iconSize.y - _iconTextSpacing;

        if (textAvailableHeight <= 0) return;

        var textSize = GetTextRenderedSize(
            new Vector2(
                contentSize.x,
                textAvailableHeight
            )
        );

        var groupSize = new Vector2(
            Mathf.Max(iconSize.x, textSize.x),
            iconSize.y + _iconTextSpacing + textSize.y
        );

        if (groupSize.x > contentSize.x || groupSize.y > contentSize.y) return;

        var groupPosition = GetAlignedPosition(contentSize, groupSize);

        _iconRenderSize = iconSize;
        _textAvailableSize = textSize;

        _iconRenderPosition = new Vector3(
            groupPosition.x + (groupSize.x - iconSize.x) * 0.5f,
            groupPosition.y
        );

        _textRenderPosition = new Vector3(
            groupPosition.x + (groupSize.x - textSize.x) * 0.5f,
            groupPosition.y + iconSize.y + _iconTextSpacing
        );

        _iconVisible = true;
        _textVisible = true;
    }

    private void OnUnityFontTextureRebuilt(Font font) => Invalidate();
}